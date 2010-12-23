import sys
import os
import shutil
import re
from glob import glob

def runProcessGenerator(command):
    streams = os.popen4(command)
    while True:
        line = streams[1].readline()
        if line:
            yield (line, None)
        else:
            break
    exitCode = streams[0].close() or streams[1].close() or 0
    yield (None, exitCode)

def runProcessRealtime(command):
    for (line, exitCode) in runProcessGenerator(command):
        if line:
            sys.stdout.write(line)
        else:
            return exitCode

def runProcess(command):
    result = []
    for (line, exitCode) in runProcessGenerator(command):
        if line:
            result.append(line)
        else:
            return (result, exitCode)

def main():
    print "-- Building package --"
    shutil.rmtree(r"dist\temp", True)
    
    os.makedirs(r"dist\temp")
    
    shutil.copy(r"bin\ShootBlues.exe", r"dist\temp")
    
    globs = (
        r"bin\*.dll",
        r"bin\*.py",
        r"bin\*.db",
        r"bin\*.wav",
        r"bin\*.mp3"
    )
        
    for g in globs:
        for fn in glob(g):
            shutil.copy(fn, r"dist\temp")
    
    print "-- Signing executable --"
    (result, exitCode) = runProcess("signtool sign /sha1 89E79C228291585064829B204E1D3571BC3CEDB0 " +
        "/d \"Shoot Blues Python Injection Toolkit\" " +
        "/du \"http://help.shootblues.com/\" " +
        "/t http://timestamp.globalsign.com/scripts/timstamp.dll " +
        r"dist\temp\ShootBlues.exe"
    )
    
    print "-- Compressing package --"   
    (result, exitCode) = runProcess(r"ext\7zip\7z.exe a -r -t7z dist\shootblues.7z .\dist\temp\*.*")
    if exitCode != 0:
        for line in result:
            sys.stdout.write(line)
        raise Exception("Compress failed.")
    
    print r"-- Done. Package built at dist\shootblues.7z. --"

main()
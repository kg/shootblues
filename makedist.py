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
    
    for fn in glob(r"bin\*.dll"):
        shutil.copy(fn, r"dist\temp")
    
    for fn in glob(r"bin\*.py"):
        shutil.copy(fn, r"dist\temp")
    
    for fn in glob(r"bin\*.db"):
        shutil.copy(fn, r"dist\temp")
    
    print "-- Compressing package --"   
    (result, exitCode) = runProcess(r"ext\7zip\7z.exe a -r -t7z dist\shootblues.7z .\dist\temp\*.*")
    if exitCode != 0:
        for line in result:
            sys.stdout.write(line)
        raise Exception("Compress failed.")
    
    print r"-- Done. Package built at dist\shootblues.7z. --"

main()
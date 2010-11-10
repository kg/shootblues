using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ShootBlues {
    public class PEReader : BinaryReader {
        public PEReader (Stream stream)
            : base(stream) {
        }

        public bool CheckHeader (string token) {
            var expected = Encoding.ASCII.GetBytes(token);
            var actual = ReadBytes(expected.Length);
            return expected.SequenceEqual(actual);
        }

        public unsafe void ReadStruct<T> (out T result) 
            where T : struct {

            int count = Marshal.SizeOf(typeof(T));
            byte[] buffer = ReadBytes(count);
            fixed (byte* bufferPtr = buffer)
                result = (T)Marshal.PtrToStructure(new IntPtr(bufferPtr), typeof(T));
        }

        public unsafe T[] ReadStructArray<T> (int count)
            where T : struct {

            var result = new T[count];
            var type = typeof(T);

            int size = Marshal.SizeOf(type);
            var buffer = new byte[size];

            fixed (byte* bufferPtr = buffer) { 
                var bufferIntPtr = new IntPtr(bufferPtr);
                for (int i = 0; i < count; i++) {
                    BaseStream.Read(buffer, 0, size);
                    result[i] = (T)Marshal.PtrToStructure(bufferIntPtr, type);
                }
            }

            return result;
        }
    }

    public class PortableExecutable {
        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public struct ImageFileHeader {
            public UInt16 Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public UInt16 Characteristics;
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        public struct ImageOptionalHeader {
            public UInt16 Magic;
            public UInt16 LinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt32 BaseOfData;
            public UInt32 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt32 OperatingSystemVersion;
            public UInt32 ImageVersion;
            public UInt32 SubsystemVersion;
            public UInt32 Reserved1;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public UInt16 Subsystem;
            public UInt16 DllCharacteristics;
            public UInt32 SizeOfStackReserve;
            public UInt32 SizeOfStackCommit;
            public UInt32 SizeOfHeapReserve;
            public UInt32 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ImageDataDirectoryHeader {
            public UInt32 VirtualAddress;
            public UInt32 Size;
        }

        public enum DataDirectoryType {
            Export = 0,
            Import = 1,
            Resource = 2,
            Exception = 3,
            Security = 4,
            BaseRelocation = 5,
            Debug = 6,
            Copyright = 7,
            GlobalPointer = 8,
            ThreadLocalStorage = 9,
            LoadConfig = 10,
            BoundImport = 11,
            ImportAddressTable = 12,
            DelayImport = 13,
            COMDescriptor = 14
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct ImageSectionHeader {
            public fixed byte _Name[8];
            public UInt32 Misc;
            public UInt32 VirtualAddress;
            public UInt32 SizeOfRawData;
            public UInt32 PointerToRawData;
            public UInt32 PointerToRelocations;
            public UInt32 PointerToLinenumbers;
            public UInt16 NumberOfRelocations;
            public UInt16 NumberOfLinenumbers;
            public UInt32 Characteristics;

            public unsafe string Name {
                get {
                    fixed (byte * name = _Name)
                        return new String((sbyte *)name);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RelocationBlockHeader {
            public UInt32 VirtualAddress;
            public UInt32 BlockSizeInclusive;
        }

        public struct Relocation {
            public UInt32 VirtualAddress;
            public RelocationType Type;
        }

        public enum RelocationType : byte {
            Absolute = 0,
            High = 1,
            Low = 2,
            HighLow = 3,
            HighAdj = 4,
            MIPS_JmpAddr = 5,
            Section = 6,
            Rel32 = 7
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ImageImportDescriptor {
            public UInt32 OriginalFirstThunkRVA;
            public UInt32 TimeDateStamp;
            public UInt32 ForwarderChain;
            public UInt32 ModuleNameRVA;
            public UInt32 FirstThunkRVA;
        }

        public struct Import {
            public string ModuleName;
            public string FunctionName;
            // The virtual address where the address of the imported function should be stored
            public UInt32 FunctionAddressDestination;
        }

        [Flags]
        public enum SectionCharacteristics : uint {
            ContainsCode = 0x00000020,
            ContainsInitializedData = 0x00000040,
            ContainsUninitializedData = 0x00000080,
            LinkInfo = 0x00000200,
            LinkRemove = 0x00000800,
            LinkCOMDAT = 0x00001000,
            MemExecute = 0x20000000,
            MemRead = 0x40000000,
            MemWrite = 0x80000000,
        }

        public class Section {
            public string Name;
            public UInt32 VirtualAddress;
            public UInt32 Size;
            public byte[] RawData;
            public SectionCharacteristics Characteristics;
        }

        public class DataDirectory {
            public DataDirectoryType Type;
            public Section Section;
            public UInt32 Offset;
            public UInt32 Size;
        }

        public ImageFileHeader FileHeader;
        public ImageOptionalHeader OptionalHeader;
        public ImageDataDirectoryHeader[] DataDirectoryHeaders;
        public ImageSectionHeader[] SectionHeaders;
        public Dictionary<DataDirectoryType, DataDirectory> DataDirectories;
        public Dictionary<string, Section> Sections;
        public Import[] Imports;
        public Relocation[] Relocations;

        public PortableExecutable (Stream stream) {
            using (var sr = new PEReader(stream)) {
                if (!sr.CheckHeader("MZ"))
                    throw new Exception("Invalid DOS header");

                stream.Seek(60, SeekOrigin.Begin);
                var headerOffset = sr.ReadUInt32();
                stream.Seek(headerOffset, SeekOrigin.Begin);

                if (!sr.CheckHeader("PE\0\0"))
                    throw new Exception("Invalid PE header");

                sr.ReadStruct(out FileHeader);
                sr.ReadStruct(out OptionalHeader);

                DataDirectoryHeaders = sr.ReadStructArray<ImageDataDirectoryHeader>((int)OptionalHeader.NumberOfRvaAndSizes);
                SectionHeaders = sr.ReadStructArray<ImageSectionHeader>(FileHeader.NumberOfSections);

                LoadSections(sr);
            }

            LoadDataDirectories();
            LoadImports();
            LoadRelocations();
        }

        public void LoadSections (PEReader sr) {
            Sections = new Dictionary<string, Section>(FileHeader.NumberOfSections);
            foreach (var sectionHeader in SectionHeaders) {
                sr.BaseStream.Seek(sectionHeader.PointerToRawData, SeekOrigin.Begin);
                var section = new Section {
                    Name = sectionHeader.Name,
                    VirtualAddress = sectionHeader.VirtualAddress,
                    Size = sectionHeader.SizeOfRawData,
                    RawData = sr.ReadBytes((int)sectionHeader.SizeOfRawData),
                    Characteristics = (SectionCharacteristics)sectionHeader.Characteristics
                };

                Sections[section.Name] = section;
            }
        }

        public void LoadDataDirectories () {
            DataDirectories = new Dictionary<DataDirectoryType, DataDirectory>(DataDirectoryHeaders.Length);
            for (int i = 0; i < DataDirectoryHeaders.Length; i++) {
                var ddh = DataDirectoryHeaders[i];
                if (ddh.Size == 0)
                    continue;

                var begin = ddh.VirtualAddress;
                var end = begin + ddh.Size;
                var section = SectionFromVirtualAddressRange(begin, end);

                DataDirectoryType type = (DataDirectoryType)i;
                DataDirectories[type] = new DataDirectory {
                    Type = type,
                    Offset = ddh.VirtualAddress - section.VirtualAddress,
                    Section = section,
                    Size = ddh.Size
                };
            }
        }

        public unsafe void LoadImports () {
            var result = new List<Import>();
            var dd = DataDirectories[DataDirectoryType.Import];
            var type = typeof(ImageImportDescriptor);
            var size = Marshal.SizeOf(type);
            int index = 0;

            fixed (byte* bufferPtr = dd.Section.RawData)
            while (true) {
                var descriptor = (ImageImportDescriptor)Marshal.PtrToStructure(
                    new IntPtr(bufferPtr + dd.Offset + (size * index)), type
                );
                if (descriptor.FirstThunkRVA == 0)
                    break;
                else
                    index += 1;

                var moduleName = new String((sbyte*)bufferPtr + descriptor.ModuleNameRVA - dd.Section.VirtualAddress);

                UInt32* rvaPtr = (UInt32*)(bufferPtr + descriptor.OriginalFirstThunkRVA - dd.Section.VirtualAddress);
                UInt32 addressDestination = descriptor.FirstThunkRVA;
                while (*rvaPtr != 0) {
                    var functionHintRva = *rvaPtr;
                    var functionNameRva = functionHintRva + 2;
                    result.Add(new Import {
                        ModuleName = moduleName,
                        FunctionName = new String((sbyte*)bufferPtr + functionNameRva - dd.Section.VirtualAddress),
                        FunctionAddressDestination = addressDestination
                    });

                    rvaPtr++;
                    addressDestination += 4;
                }
            }

            Imports = result.ToArray();
        }

        public unsafe void LoadRelocations () {
            var result = new List<Relocation>();

            var dd = DataDirectories[DataDirectoryType.BaseRelocation];
            var type = typeof(RelocationBlockHeader);
            var size = Marshal.SizeOf(type);
            uint offset = dd.Offset;

            fixed (byte* bufferPtr = dd.Section.RawData)
            while (true) {
                if ((offset + size) >= dd.Size)
                    break;

                var blockHeader = (RelocationBlockHeader)Marshal.PtrToStructure(
                    new IntPtr(bufferPtr + offset), type
                );

                uint numRelocations = (blockHeader.BlockSizeInclusive - (uint)size) / 2;
                for (uint i = 0; i < numRelocations; i++) {
                    UInt16 relocationRaw = *(UInt16*)(bufferPtr + offset + size + (i * 2));
                    result.Add(new Relocation {
                        Type = (RelocationType)((relocationRaw >> 12) & 0xf),
                        VirtualAddress = (UInt32)((relocationRaw & 0xfff) + blockHeader.VirtualAddress)
                    });
                }

                offset += blockHeader.BlockSizeInclusive;
            }

            Relocations = result.ToArray();
        }

        public void Rebase (UInt32 newBaseAddress) {
            var oldBaseAddress = OptionalHeader.ImageBase;
            long delta = (long)newBaseAddress - (long)oldBaseAddress;

            OptionalHeader.ImageBase = newBaseAddress;

            int numRelocations = 0;
            byte[] buffer;
            foreach (var relocation in Relocations) {
                if (relocation.Type == RelocationType.Absolute)
                    continue;
                else if (relocation.Type != RelocationType.HighLow)
                    throw new Exception(String.Format("Relocation type not implemented: {0}", relocation.Type));

                var section = SectionFromVirtualAddress(relocation.VirtualAddress);
                var offset = relocation.VirtualAddress - section.VirtualAddress;
                UInt32 value = BitConverter.ToUInt32(section.RawData, (int)offset);
                value = (UInt32)(value + delta);
                buffer = BitConverter.GetBytes(value);
                Array.Copy(buffer, 0, section.RawData, offset, buffer.Length);

                numRelocations += 1;
            }

            Console.WriteLine("Processed {0} relocation(s) to rebase to {0:x8}.", numRelocations, newBaseAddress);
        }

        public void ResolveImports () {
            int numImports = 0;

            foreach (var import in Imports) {
                var hModule = Win32.LoadLibrary(import.ModuleName);
                if (hModule == IntPtr.Zero)
                    throw new Exception(String.Format("Module load failed: {0}", import.ModuleName));

                try {
                    var procAddress = Win32.GetProcAddress(hModule, import.FunctionName);
                    if (procAddress == 0)
                        throw new Exception(String.Format("Unresolved import: {0}:{1}", import.ModuleName, import.FunctionName));

                    var section = SectionFromVirtualAddress(import.FunctionAddressDestination);
                    var offset = import.FunctionAddressDestination - section.VirtualAddress;
                    var bytes = BitConverter.GetBytes(procAddress);
                    Array.Copy(bytes, 0, section.RawData, offset, bytes.Length);

                    numImports += 1;
                } finally {
                    Win32.FreeLibrary(hModule);
                }
            }

            Console.WriteLine("Processed {0} import(s).", numImports);
        }

        public Section SectionFromVirtualAddressRange (UInt32 addressBegin, UInt32 addressEnd) {
            return Sections.Values.First(
                (s) => (addressBegin >= s.VirtualAddress) && (addressEnd < s.VirtualAddress + s.Size)
            );
        }

        public Section SectionFromVirtualAddress (UInt32 address) {
            return SectionFromVirtualAddressRange(address, address);
        }
    }
}

using System;

namespace SakerCore.IO
{

#pragma warning disable CS1591
    public interface IBinnaryOperation
    { 

        void WriteLength(int value);
        int ReadLength();

        void WriteValue(byte value);
        void WriteValue(byte[] value);
        void WriteValue(DateTime value);
        void WriteValue(decimal value);
        void WriteValue(double value);
        void WriteValue(float value);
        void WriteValue(Guid value);
        void WriteValue(short value);
        void WriteValue(int value);
        void WriteValue(long value);
        void WriteValue(sbyte value);
        void WriteValue(string value);
        void WriteValue(TimeSpan value);
        void WriteValue(ushort value);
        void WriteValue(uint value);
        void WriteValue(ulong value);

         void ReadValue(out byte value);
         void ReadValue(out byte[] value);
         void ReadValue(out DateTime value);
         void ReadValue(out decimal value);
         void ReadValue(out double value);
         void ReadValue(out float value);
         void ReadValue(out Guid value);
         void ReadValue(out short value);
         void ReadValue(out int value);
         void ReadValue(out long value); 
         void ReadValue(out sbyte value);
         void ReadValue(out string value);
         void ReadValue(out TimeSpan value);
         void ReadValue(out ushort value);
         void ReadValue(out uint value);
         void ReadValue(out ulong value);

    }
}
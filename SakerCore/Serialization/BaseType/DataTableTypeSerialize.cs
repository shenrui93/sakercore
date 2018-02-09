/***************************************************************************
 * 
 * 创建时间：   2016/7/11 10:33:46
 * 创建人员：   沈瑞
 * CLR版本号：  4.0.30319.42000
 * 备注信息：   未填写备注信息
 * 
 * *************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using SakerCore.Serialization.BigEndian;

namespace SakerCore.Serialization.BaseType
{
    /// <summary>
    /// 类DataTableTypeSerialize的注释信息
    /// </summary>
    internal class DataTableTypeSerialize : TypeSerializationBase<DataTable>
    {
        public override void Serialize(DataTable obj, System.IO.Stream stream)
        {
            Write(obj, stream);
        }
        public override DataTable Deserialize(System.IO.Stream stream)
        {
            return Read(stream);
        } 
        public static void Write(DataTable obj, System.IO.Stream stream)
        {
            BigEndianPrimitiveTypeSerializer.Instance.WriteValue(stream, obj);
        }
        public static DataTable Read(System.IO.Stream stream)
        {
            DataTable tb;
            BigEndianPrimitiveTypeSerializer.Instance.ReadValue(stream, out tb);
            return tb; 
        } 
        public DataTableTypeSerialize()
        {
            this._serializerType = typeof(DataTable);
            this._serializerName = this._serializerType.FullName;
        }



        public override System.Reflection.MethodInfo GetReaderMethod()
        {
            Func<System.IO.Stream, DataTable> reader = Read;
            return reader.Method;
        }
        public override System.Reflection.MethodInfo GetWriterMethod()
        {
            Action<DataTable, System.IO.Stream> writer = Write;
            return writer.Method;
        }




    }
}

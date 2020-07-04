using System;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
    public class WzLongProperty : WzImageProperty
    {
        #region Fields

        internal string name;
        internal long val;

        #endregion

        #region Inherited Members

        public override void SetValue(object value)
        {
            val = Convert.ToInt64(value);
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzLongProperty(Name, val);
            return clone;
        }

        public override object WzValue => Value;

        /*/// <summary>
        /// The image that this property is contained in
        /// </summary>
        public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/
        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.Long;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.Write((byte) 20);
            writer.WriteCompressedLong(Value);
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
        }

        #endregion

        #region Custom Members

        /// <summary>
        /// The value of the property
        /// </summary>
        public long Value
        {
            get => val;
            set => val = value;
        }

        /// <summary>
        /// Creates a blank WzCompressedIntProperty
        /// </summary>
        public WzLongProperty()
        {
        }

        /// <summary>
        /// Creates a WzCompressedIntProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzLongProperty(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Creates a WzCompressedIntProperty with the specified name and value
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public WzLongProperty(string name, long value)
        {
            this.name = name;
            val = value;
        }

        #endregion

        #region Cast Values

        public override float GetFloat()
        {
            return val;
        }

        public override double GetDouble()
        {
            return val;
        }

        public override long GetLong()
        {
            return val;
            
        }

        public override int GetInt()
        {
            return (int) val;
        }

        public override short GetShort()
        {
            return (short) val;
        }

        public override string ToString()
        {
            return val.ToString();
        }

        #endregion
    }
}
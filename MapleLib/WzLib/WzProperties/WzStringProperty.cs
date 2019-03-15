using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
    /// <summary>
    /// A property with a string as a value
    /// </summary>
    public class WzStringProperty : WzImageProperty
    {
        #region Fields

        internal string name, val;

        internal WzObject parent;
        //internal WzImage imgParent;

        #endregion

        #region Inherited Members

        public override void SetValue(object value)
        {
            val = (string) value;
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzStringProperty(name, val);
            return clone;
        }

        public override object WzValue => Value;

        /// <summary>
        /// The parent of the object
        /// </summary>
        public override WzObject Parent
        {
            get => parent;
            internal set => parent = value;
        }

        /*/// <summary>
        /// The image that this property is contained in
        /// </summary>
        public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/
        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.String;

        /// <summary>
        /// The name of the property
        /// </summary>
        public override string Name
        {
            get => name;
            set => name = value;
        }

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.Write((byte) 8);
            writer.WriteStringValue(Value, 0, 1);
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
            val = null;
        }

        #endregion

        #region Custom Members

        /// <summary>
        /// The value of the property
        /// </summary>
        public string Value
        {
            get => val;
            set => val = value;
        }

        /// <summary>
        /// Creates a blank WzStringProperty
        /// </summary>
        public WzStringProperty()
        {
        }

        /// <summary>
        /// Creates a WzStringProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzStringProperty(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Creates a WzStringProperty with the specified name and value
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public WzStringProperty(string name, string value)
        {
            this.name = name;
            val = value;
        }

        #endregion

        #region Cast Values

        public override string GetString()
        {
            return val;
        }

        public override string ToString()
        {
            return val;
        }

        #endregion
    }
}
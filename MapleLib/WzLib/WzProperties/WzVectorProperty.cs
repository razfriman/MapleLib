using System.Drawing;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
    /// <summary>
    /// A property that contains an x and a y value
    /// </summary>
    public class WzVectorProperty : WzExtended
    {
        #region Fields

        internal string name;
        internal WzIntProperty x, y;

        internal WzObject parent;
        //internal WzImage imgParent;

        #endregion

        #region Inherited Members

        public override void SetValue(object value)
        {
            if (value is Point)
            {
                x.Value = ((Point) value).X;
                y.Value = ((Point) value).Y;
            }
            else
            {
                x.Value = ((Size) value).Width;
                y.Value = ((Size) value).Height;
            }
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzVectorProperty(name, x, y);
            return clone;
        }

        public override object WzValue => new Point(x.Value, y.Value);

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
        /// The name of the property
        /// </summary>
        public override string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.Vector;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.WriteStringValue("Shape2D#Vector2D", 0x73, 0x1B);
            writer.WriteCompressedInt(X.Value);
            writer.WriteCompressedInt(Y.Value);
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
            x.Dispose();
            x = null;
            y.Dispose();
            y = null;
        }

        #endregion

        #region Custom Members

        /// <summary>
        /// The X value of the Vector2D
        /// </summary>
        public WzIntProperty X
        {
            get => x;
            set => x = value;
        }

        /// <summary>
        /// The Y value of the Vector2D
        /// </summary>
        public WzIntProperty Y
        {
            get => y;
            set => y = value;
        }

        /// <summary>
        /// The Point of the Vector2D created from the X and Y
        /// </summary>
        public Point Pos => new Point(X.Value, Y.Value);

        /// <summary>
        /// Creates a blank WzVectorProperty
        /// </summary>
        public WzVectorProperty()
        {
        }

        /// <summary>
        /// Creates a WzVectorProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzVectorProperty(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Creates a WzVectorProperty with the specified name, x and y
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="x">The x value of the vector</param>
        /// <param name="y">The y value of the vector</param>
        public WzVectorProperty(string name, WzIntProperty x, WzIntProperty y)
        {
            this.name = name;
            this.x = x;
            this.y = y;
        }

        #endregion

        #region Cast Values

        public override Point GetPoint() => new Point(x.Value, y.Value);

        public override string ToString() => $"X: {x.Value}, Y: {y.Value}";

        #endregion
    }
}
﻿using System;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
    /// <summary>
    /// A property that's value is null
    /// </summary>
    public class WzNullProperty : WzImageProperty
    {
        #region Fields

        internal string name;

        internal WzObject parent;
        //internal WzImage imgParent;

        #endregion

        #region Inherited Members

        public override void SetValue(object value)
        {
            throw new NotImplementedException();
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzNullProperty(name);
            return clone;
        }

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
        public override WzPropertyType PropertyType => WzPropertyType.Null;

        /// <summary>
        /// The name of the property
        /// </summary>
        /// 
        public override string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// The WzObjectType of the property
        /// </summary>
        public override WzObjectType ObjectType => WzObjectType.Property;

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.Write((byte) 0);
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
        }

        #endregion

        #region Custom Members

        /// <summary>
        /// Creates a blank WzNullProperty
        /// </summary>
        public WzNullProperty()
        {
        }

        /// <summary>
        /// Creates a WzNullProperty with the specified name
        /// </summary>
        /// <param name="propName">The name of the property</param>
        public WzNullProperty(string propName)
        {
            name = propName;
        }

        #endregion
    }
}
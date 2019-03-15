using System;
using System.DrawingCore;
using Point = System.Drawing.Point;

namespace MapleLib.WzLib
{
    /// <summary>
    /// An abstract class for wz objects
    /// </summary>
    public abstract class WzObject : IDisposable
    {
        public abstract void Dispose();

        /// <summary>
        /// The name of the object
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// The WzObjectType of the object
        /// </summary>
        public abstract WzObjectType ObjectType { get; }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        public abstract WzObject Parent { get; internal set; }

        /// <summary>
        /// Returns the parent WZ File
        /// </summary>
        public abstract WzFile WzFileParent { get; }

        public WzObject this[string name]
        {
            get
            {
                if (this is WzFile)
                {
                    return ((WzFile) this)[name];
                }

                if (this is WzDirectory)
                {
                    return ((WzDirectory) this)[name];
                }

                if (this is WzImage)
                {
                    return ((WzImage) this)[name];
                }

                if (this is WzImageProperty)
                {
                    return ((WzImageProperty) this)[name];
                }

                throw new NotImplementedException();
            }
        }

        public string FullPath
        {
            get
            {
                if (this is WzFile)
                {
                    return ((WzFile) this).WzDirectory.Name;
                }

                var result = Name;
                var currObj = this;
                while (currObj.Parent != null)
                {
                    currObj = currObj.Parent;
                    result = currObj.Name + @"\" + result;
                }

                return result;
            }
        }

        public virtual object WzValue => null;

        public abstract void Remove();

        #region Cast Values

        public virtual int GetInt()
        {
            throw new NotImplementedException();
        }

        public virtual short GetShort()
        {
            throw new NotImplementedException();
        }

        public virtual long GetLong()
        {
            throw new NotImplementedException();
        }

        public virtual float GetFloat()
        {
            throw new NotImplementedException();
        }

        public virtual double GetDouble()
        {
            throw new NotImplementedException();
        }

        public virtual string GetString()
        {
            throw new NotImplementedException();
        }

        public virtual Point GetPoint()
        {
            throw new NotImplementedException();
        }

        public virtual Bitmap GetBitmap()
        {
            throw new NotImplementedException();
        }

        public virtual byte[] GetBytes()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
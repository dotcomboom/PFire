﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFire.Protocol.XFireAttributes
{
    public abstract class MapAttribute<T> : XFireAttribute
    {

        /*private XFireAttribute cachedKeyAttribute;

        public MapAttribute()
        {
            cachedKeyAttribute = XFireAttributeFactory.Instance.GetAttribute(typeof(T));
        }
        */
        public override Type AttributeType
        {
            get { return typeof(Dictionary<T, dynamic>); }
        }

        public override abstract byte AttributeTypeId { get; }

        public override dynamic ReadValue(BinaryReader reader)
        {
            var values = new Dictionary<T, dynamic>();
            var mapLength = reader.ReadByte();
            var keyAttribute = XFireAttributeFactory.Instance.GetAttribute(typeof(T));

            for (int i = 0; i < mapLength; i++)
            {
                //var key = cachedKeyAttribute.ReadValue(reader);

                // TODO: Fix hack
                // Stupid protocol decides to not be nice and expect an 8bit string length prefix instead of the normal 16 for string key mapped types

                var key = keyAttribute is StringAttribute ? ReadInt8StringLengthHack(reader) : keyAttribute.ReadValue(reader);

                var type = reader.ReadByte();
                var attribute = XFireAttributeFactory.Instance.GetAttribute(type);

                values.Add(key, attribute.ReadValue(reader));
            }
            return values;
        }

        public override void WriteValue(BinaryWriter writer, dynamic data)
        {
            var mapLength = (byte)data.Count;
            var keyAttribute = XFireAttributeFactory.Instance.GetAttribute(typeof(T));

            writer.Write(mapLength);

            foreach (var pair in data)
            {
                var key = pair.Key;
                var value = pair.Value;
                var attribute = XFireAttributeFactory.Instance.GetAttribute(value.GetType());

                //cachedKeyAttribute.WriteValue(writer, key);

                // TODO: Fix hack
                // Stupid protocol decides to not be nice and expect an 8bit string length prefix instead of 16 for string key mapped types
                if (keyAttribute is StringAttribute)
                {
                    WriteInt8StringLengthHack(writer, (string)key);
                }
                else
                {
                    keyAttribute.WriteValue(writer, key);
                }

                attribute.WriteType(writer);
                attribute.WriteValue(writer, value);
            }
        }

        private void WriteInt8StringLengthHack(BinaryWriter writer, string value)
        {
            writer.Write((byte)value.Length);
            writer.Write(Encoding.UTF8.GetBytes(value));
        }

        private string ReadInt8StringLengthHack(BinaryReader reader)
        {
            var length = reader.ReadByte();
            return Encoding.UTF8.GetString(reader.ReadBytes(length));
        }
    }
}

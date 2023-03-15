using System;
using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A navigation format used in DeS and DS1 that defines a basic graph of connected volumes. Extension: .mcp
    /// </summary>
    public class MCP : SoulsFile<MCP> {
        /// <summary>
        /// True for DeS, false for DS1.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public int Unk04 { get; set; }

        /// <summary>
        /// Interconnected volumes making up a general graph of the map.
        /// </summary>
        public List<Room> Rooms { get; set; }

        /// <summary>
        /// Creates an empty MCP.
        /// </summary>
        public MCP() => this.Rooms = new List<Room>();

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = true;
            this.BigEndian = br.AssertInt32(2, 0x2000000) == 2;
            br.BigEndian = this.BigEndian;
            this.Unk04 = br.ReadInt32();
            int roomCount = br.ReadInt32();
            int roomsOffset = br.ReadInt32();

            br.Position = roomsOffset;
            this.Rooms = new List<Room>(roomCount);
            for (int i = 0; i < roomCount; i++) {
                this.Rooms.Add(new Room(br));
            }
        }

        /// <summary>
        /// Verifies that there are no null references or invalid indices.
        /// </summary>
        public override bool Validate(out Exception ex) {
            if (!ValidateNull(this.Rooms, $"{nameof(this.Rooms)} may not be null.", out ex)) {
                return false;
            }

            for (int i = 0; i < this.Rooms.Count; i++) {
                Room room = this.Rooms[i];
                if (!ValidateNull(room, $"{nameof(this.Rooms)}[{i}]: {nameof(Room)} may not be null.", out ex)
                    || !ValidateNull(room.ConnectedRoomIndices, $"{nameof(this.Rooms)}[{i}]: {nameof(Room.ConnectedRoomIndices)} may not be null.", out ex)) {
                    return false;
                }

                for (int j = 0; j < room.ConnectedRoomIndices.Count; j++) {
                    int roomIndex = room.ConnectedRoomIndices[j];
                    if (!ValidateIndex(this.Rooms.Count, roomIndex, $"{nameof(this.Rooms)}[{i}].{nameof(Room.ConnectedRoomIndices)}[{j}]: Index out of range: {roomIndex}", out ex)) {
                        return false;
                    }
                }
            }

            ex = null;
            return true;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;
            bw.WriteInt32(2);
            bw.WriteInt32(this.Unk04);
            bw.WriteInt32(this.Rooms.Count);
            bw.ReserveInt32("RoomsOffset");

            long[] indicesOffsets = new long[this.Rooms.Count];
            for (int i = 0; i < this.Rooms.Count; i++) {
                indicesOffsets[i] = bw.Position;
                bw.WriteInt32s(this.Rooms[i].ConnectedRoomIndices);
            }

            bw.FillInt32("RoomsOffset", (int)bw.Position);
            for (int i = 0; i < this.Rooms.Count; i++) {
                this.Rooms[i].Write(bw, indicesOffsets[i]);
            }
        }

        /// <summary>
        /// A volume of space with connections to other rooms.
        /// </summary>
        public class Room {
            /// <summary>
            /// The ID of the map the room is in, where mAA_BB_CC_DD is packed into bytes AABBCCDD of the uint.
            /// </summary>
            public uint MapID { get; set; }

            /// <summary>
            /// Index of the room among rooms with the same map ID, for MCPs that span multiple maps.
            /// </summary>
            public int LocalIndex { get; set; }

            /// <summary>
            /// Minimum extent of the room.
            /// </summary>
            public Vector3 BoundingBoxMin { get; set; }

            /// <summary>
            /// Maximum extent of the room.
            /// </summary>
            public Vector3 BoundingBoxMax { get; set; }

            /// <summary>
            /// Indices of rooms connected to this one.
            /// </summary>
            public List<int> ConnectedRoomIndices { get; set; }

            /// <summary>
            /// Creates a Room with default values.
            /// </summary>
            public Room() => this.ConnectedRoomIndices = new List<int>();

            internal Room(BinaryReaderEx br) {
                this.MapID = br.ReadUInt32();
                this.LocalIndex = br.ReadInt32();
                int indexCount = br.ReadInt32();
                int indicesOffset = br.ReadInt32();
                this.BoundingBoxMin = br.ReadVector3();
                this.BoundingBoxMax = br.ReadVector3();

                this.ConnectedRoomIndices = new List<int>(br.GetInt32s(indicesOffset, indexCount));
            }

            internal void Write(BinaryWriterEx bw, long indicesOffset) {
                bw.WriteUInt32(this.MapID);
                bw.WriteInt32(this.LocalIndex);
                bw.WriteInt32(this.ConnectedRoomIndices.Count);
                bw.WriteInt32((int)indicesOffset);
                bw.WriteVector3(this.BoundingBoxMin);
                bw.WriteVector3(this.BoundingBoxMax);
            }
        }
    }
}

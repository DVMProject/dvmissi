﻿/**
* Digital Voice Modem - ISSI
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / ISSI
*
*/
/*
*   Copyright (C) 2025 by Bryan Biedenkapp N2PLL
*
*   This program is free software: you can redistribute it and/or modify
*   it under the terms of the GNU Affero General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU Affero General Public License for more details.
*/

using System;

namespace dvmissi.ISSI.RTP
{
    /// <summary>
    /// Implements a P25 PTT control packet.
    /// </summary>
    /// 
    /// Byte 0                   1                   2                   3
    /// Bit  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ///     +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     |                  WACN ID              |       System ID       |
    ///     +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     |            Unit ID                            |      TP       |
    ///     +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    public class PTTControl
    {
        public const int LENGTH = 8;

        /// <summary>
        /// Wide-Area Communication Network ID
        /// </summary>
        public uint WACN
        {
            get;
            set;
        }

        /// <summary>
        /// System ID.
        /// </summary>
        public ushort SysId
        {
            get;
            set;
        }

        /// <summary>
        /// Unit ID.
        /// </summary>
        public uint UnitID
        {
            get;
            set;
        }

        /// <summary>
        /// Transmit Priority.
        /// </summary>
        public byte Priority
        {
            get;
            set;
        }

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="PTTControl"/> class.
        /// </summary>
        public PTTControl()
        {
            WACN = 0;
            SysId = 0;
            UnitID = 0;
            Priority = 0;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PTTControl"/> class.
        /// </summary>
        /// <param name="data"></param>
        public PTTControl(byte[] data) : this()
        {
            Decode(data);
        }

        /// <summary>
        /// Decode a block.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Decode(byte[] data)
        {
            if (data == null)
                return false;

            ulong value = 0U;

            // combine bytes into ulong (8 byte) value
            value = data[0U];
            value = (value << 8) + data[1U];
            value = (value << 8) + data[2U];
            value = (value << 8) + data[3U];
            value = (value << 8) + data[4U];
            value = (value << 8) + data[5U];
            value = (value << 8) + data[6U];
            value = (value << 8) + data[7U];

            WACN = (uint)((value >> 44) & 0xFFFFFU);                            // WACN
            SysId = (ushort)((value >> 32) & 0xFFFU);                           // System ID
            UnitID = (uint)((value >> 8) & 0xFFFFFFU);                          // Unit ID
            Priority = (byte)(value & 0xFFU);                                   // Priority

            return true;
        }

        /// <summary>
        /// Encode a block.
        /// </summary>
        /// <param name="data"></param>
        public void Encode(ref byte[] data)
        {
            if (data == null)
                return;

            ulong value = 0;

            value = (ulong)(WACN & 0xFFFFFU);                                   // WACN
            value = (value << 12) + (SysId & 0xFFFU);                           // System ID
            value = (value << 24) + (UnitID & 0xFFFFFFU);                       // Unit ID
            value = (value << 8) + (Priority & 0xFFU);                          // Priority

            // split ulong (8 byte) value into bytes
            data[0U] = (byte)((value >> 56) & 0xFFU);
            data[1U] = (byte)((value >> 48) & 0xFFU);
            data[2U] = (byte)((value >> 40) & 0xFFU);
            data[3U] = (byte)((value >> 32) & 0xFFU);
            data[4U] = (byte)((value >> 24) & 0xFFU);
            data[5U] = (byte)((value >> 16) & 0xFFU);
            data[6U] = (byte)((value >> 8) & 0xFFU);
            data[7U] = (byte)((value >> 0) & 0xFFU);
        }
    } // public class PTTControl
} // namespace dvmissi.ISSI.RTP

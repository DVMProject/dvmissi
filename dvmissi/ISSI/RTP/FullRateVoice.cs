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

using fnecore.P25;

namespace dvmissi.ISSI.RTP
{
    /// <summary>
    /// Implements a P25 full rate voice packet.
    /// </summary>
    /// 
    /// Byte 0                   1                   2                   3
    /// Bit  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ///     +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     |       FT      |       U0(b11-0)       |      U1(b11-0)        |
    ///     +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     |        U2(b10-0)      |      U3(b11-0)        |   U4(b10-3)   |
    ///     +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     |  U4 |     U5(b10-0)       |     U6(b10-0)       |  U7(b6-0)   |
    ///     +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
    ///     |  Et | Er  |M|L|E|  E1 |SF |rs | Additional Frame Type Specific|
    ///     |     |     | | |4|     |   |vd | Data(variable length)         |
    ///     +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
    ///     |             Additional Frame Type Specific Data               |
    ///     |                        (variable length)                      |
    ///     +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    public class FullRateVoice
    {
        public const int LENGTH = 14;
        private const int IMBE_BUF_LEN = 11;

        /// <summary>
        /// Frame type.
        /// </summary>
        public byte FrameType
        {
            get;
            set;
        }

        /// <summary>
        /// IMBE.
        /// </summary>
        public byte[] IMBE
        {
            get;
            set;
        }

        /// <summary>
        /// Total errors detected in the frame.
        /// </summary>
        public byte TotalErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Flag indicating the frame should be muted.
        /// </summary>
        public bool MuteFrame
        {
            get;
            set;
        }

        /// <summary>
        /// Flag indicating the frame was lost.
        /// </summary>
        public bool LostFrame
        {
            get;
            set;
        }

        /// <summary>
        /// Superframe Counter.
        /// </summary>
        public byte SuperFrameCnt
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public byte[] AdditionalFrameData
        {
            get;
            set;
        }

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="FullRateVoice"/> class.
        /// </summary>
        public FullRateVoice()
        {
            FrameType = P25ISSI.P25_DFSI_LDU1_VOICE1;
            TotalErrors = 0;
            MuteFrame = false;
            LostFrame = false;
            SuperFrameCnt = 0;
            AdditionalFrameData = null;

            IMBE = new byte[IMBE_BUF_LEN];
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FullRateVoice"/> class.
        /// </summary>
        /// <param name="data"></param>
        public FullRateVoice(byte[] data) : this()
        {
            Decode(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            if (AdditionalFrameData != null)
                return LENGTH + AdditionalFrameData.Length;
            else
                return LENGTH;
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

            IMBE = new byte[IMBE_BUF_LEN];

            FrameType = (byte)(data[0U] & 0xFFU);                               // Frame Type
            for (int i = 0; i < IMBE_BUF_LEN; i++)
                IMBE[i] = data[i + 1U];                                         // IMBE

            TotalErrors = (byte)((data[12U] >> 5) & 0x07U);                     // Total Errors
            MuteFrame = (data[12U] & 0x02U) == 0x02U;                           // Mute Frame Flag
            LostFrame = (data[12U] & 0x01U) == 0x01U;                           // Lost Frame Flag
            SuperFrameCnt = (byte)((data[13U] >> 2) & 0x03U);                   // Superframe Counter

            // extract additional frame data
            if (data.Length > LENGTH)
            {
                AdditionalFrameData = new byte[data.Length - LENGTH];
                Buffer.BlockCopy(data, LENGTH, AdditionalFrameData, 0, AdditionalFrameData.Length);
            }
            else
                AdditionalFrameData = null;

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

            data[0U] = FrameType;                                               // Frame Type
            for (int i = 0; i < IMBE_BUF_LEN; i++)
                data[i + 1U] = IMBE[i];                                         // IMBE

            data[12U] = (byte)(((TotalErrors & 0x07U) << 5) +                   // Total Errors
                (MuteFrame ? 0x02U : 0x00U) +                                   // Mute Frame Flag
                (LostFrame ? 0x01U : 0x00U));                                   // Lost Frame Flag
            data[13U] = (byte)((SuperFrameCnt & 0x03U) << 2);                   // Superframe Count

            if (AdditionalFrameData != null)
            {
                if (data.Length >= 14U + AdditionalFrameData.Length)
                    Buffer.BlockCopy(AdditionalFrameData, 0, data, 14, AdditionalFrameData.Length);
            }
        }
    } // public class FullRateVoice
} // namespace dvmissi.ISSI.RTP

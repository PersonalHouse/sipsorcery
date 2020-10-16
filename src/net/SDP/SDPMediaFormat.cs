﻿//-----------------------------------------------------------------------------
// Filename: SDPMediaFormat.cs
//
// Description: 
//
// Author(s):
// Aaron Clauson (aaron@sipsorcery.com)
//
// History:
// ??	Aaron Clauson	Created, Hobart, Australia.
//
// License: 
// BSD 3-Clause "New" or "Revised" License, see included LICENSE.md file.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SIPSorceryMedia.Abstractions.V1;

namespace SIPSorcery.Net
{
    /// <summary>
    /// A list of standard media formats that can be identified by an ID if
    /// there is no qualifying format attribute provided.
    /// </summary>
    public enum SDPMediaFormatsEnum
    {
        PCMU = 0,   // Audio.
        GSM = 3,    // Audio.
        G723 = 4,   // Audio.
        G722 = 9,   // Audio.
        PCMA = 8,   // Audio.
        G729 = 18,  // Audio.

        JPEG = 26,  // Video
        H263 = 34,  // Video.

        // Payload identifiers 96–127 are used for payloads defined dynamically 
        // during a session.
        // The types following are standard media types that do not have a 
        // recognised ID but still do have recognised properties. The ID
        // assigned is arbitrary and should not necessarily be used in SDP.
        VP9 = 98,
        VP8 = 100,  // Video.
        Telephone_Event = 101,
        H264 = 102,
        H265 = 103,
        OPUS = 111,
        L16 = 119,  // Audio 16 bit signed linear (uncompressed).

        Unknown = 999,
    }

    public class SDPMediaFormatInfo
    {
        /// <summary>
        /// Attempts to get the clock rate of known payload types.
        /// </summary>
        /// <param name="mediaType">The media type to get the clock rate for.</param>
        /// <returns>An integer representing the payload type's sampling frequency or 0
        /// if it's not known.</returns>
        public static int GetClockRate(SDPMediaFormatsEnum payloadType)
        {
            switch (payloadType)
            {
                case SDPMediaFormatsEnum.PCMU:
                case SDPMediaFormatsEnum.PCMA:
                    return 8000;
                case SDPMediaFormatsEnum.G722:
                    return 16000;
                case SDPMediaFormatsEnum.VP8:
                case SDPMediaFormatsEnum.VP9:
                case SDPMediaFormatsEnum.H264:
                    return 90000;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Attempts to get the RTP clock rate of known payload types. Generally this will be the same
        /// as the clock rate but in some cases for seemingly historical reasons they are different.
        /// </summary>
        /// <param name="mediaType">The media type to get the clock rate for.</param>
        /// <returns>An integer representing the payload type's RTP timestamp frequency or 0
        /// if it's not known.</returns>
        public static int GetRtpClockRate(SDPMediaFormatsEnum payloadType)
        {
            switch (payloadType)
            {
                case SDPMediaFormatsEnum.G722:
                    return 8000;
                case SDPMediaFormatsEnum.OPUS:
                    return 48000;
                default:
                    return GetClockRate(payloadType);
            }
        }

        /// <summary>
        /// Attempts to get the RTP clock rate of known payload types. Generally this will be the same
        /// as the clock rate but in some cases for seemingly historical reasons they are different
        /// </summary>
        /// <param name="mediaType">The media type to get the clock rate for.</param>
        /// <returns>An integer representing the payload type's RTP timestamp frequency or 0
        /// if it's not known.</returns>
        public static int GetRtpDefaultClockRate(AudioCodecsEnum codec)
        {
            switch (codec)
            {
                case AudioCodecsEnum.PCMA:
                case AudioCodecsEnum.PCMU:
                case AudioCodecsEnum.G722:
                    return 8000;
                case AudioCodecsEnum.OPUS:
                    return 48000;
                default:
                    return 8000;
            }
        }

        public static AudioSamplingRatesEnum GetRtpClockRate(AudioFormat format)
        {
            switch(format.Codec)
            {
                case AudioCodecsEnum.PCMA:
                case AudioCodecsEnum.PCMU:
                    return AudioSamplingRatesEnum.Rate8KHz;
                case AudioCodecsEnum.G722:
                case AudioCodecsEnum.L16:
                    switch (format.FormatAttribute)
                    {
                        case var att when string.IsNullOrWhiteSpace(att):
                            return AudioSamplingRatesEnum.Rate8KHz;
                        case var att when SDPFormatAttribute.TryParseFormatAttribute(att, out var fAtt):
                            return fAtt.ClockRate == 16000 ? AudioSamplingRatesEnum.Rate16KHz : AudioSamplingRatesEnum.Rate8KHz;
                        default:
                            return AudioSamplingRatesEnum.Rate8KHz;
                    }
                default:
                    return AudioSamplingRatesEnum.Rate8KHz;
            }
        }

        /// <summary>
        /// Maps an audio SDP media type to an media abstraction layer audio format.
        /// </summary>
        /// <param name="sdpFormat">The SDP format to map to an audio format.</param>
        /// <returns>An audio format value.</returns>
        public static AudioFormat GetAudioFormatForSdpFormat(SDPMediaFormat sdpFormat)
        {
            switch(sdpFormat.FormatCodec)
            {
                case SDPMediaFormatsEnum.G722:
                    return new AudioFormat(AudioCodecsEnum.G722, sdpFormat.FormatAttribute, sdpFormat.FormatParameterAttribute);
                case SDPMediaFormatsEnum.PCMA:
                    return new AudioFormat(AudioCodecsEnum.PCMA, sdpFormat.FormatAttribute, sdpFormat.FormatParameterAttribute);
                case SDPMediaFormatsEnum.PCMU:
                    return new AudioFormat(AudioCodecsEnum.PCMU, sdpFormat.FormatAttribute, sdpFormat.FormatParameterAttribute);
                case SDPMediaFormatsEnum.OPUS:
                    return new AudioFormat(AudioCodecsEnum.OPUS, sdpFormat.FormatAttribute, sdpFormat.FormatParameterAttribute);
                case SDPMediaFormatsEnum.L16:
                    return new AudioFormat(AudioCodecsEnum.L16, sdpFormat.FormatAttribute, sdpFormat.FormatParameterAttribute);
                default:
                    return new AudioFormat(Convert.ToInt32(sdpFormat.FormatID), sdpFormat.Name, sdpFormat.FormatAttribute, sdpFormat.FormatParameterAttribute);
            }
        }

        /// <summary>
        /// Maps a video SDP media type to an media abstraction layer video format.
        /// </summary>
        /// <param name="sdpFormat">The SDP format to map to a video format.</param>
        /// <returns>A video format value.</returns>
        public static VideoFormat GetVideoFormatForSdpFormat(SDPMediaFormat sdpFormat)
        {
            switch (sdpFormat.FormatCodec)
            {
                case SDPMediaFormatsEnum.H264:
                    return new VideoFormat(VideoCodecsEnum.H264, sdpFormat.FormatAttribute, sdpFormat.FormatParameterAttribute);
                case SDPMediaFormatsEnum.VP8:
                    return new VideoFormat(VideoCodecsEnum.VP8, sdpFormat.FormatAttribute, sdpFormat.FormatParameterAttribute);
                default:
                    return new VideoFormat(Convert.ToInt32(sdpFormat), sdpFormat.Name, sdpFormat.FormatAttribute, sdpFormat.FormatParameterAttribute);
            }
        }
    }

    public struct SDPFormatAttribute
    {
        public static readonly SDPFormatAttribute Empty = new SDPFormatAttribute { ClockRate = 0, Name = null };

        //public string FormatID;
        public string Name;
        public int ClockRate;

        public static bool TryParseFormatAttribute(string attribute, out SDPFormatAttribute formatAttribute)
        {
            formatAttribute = SDPFormatAttribute.Empty;

            Match attributeMatch = Regex.Match(attribute, @"(?<name>[a-zA-Z0-9\-]+)/(?<clockrate>\d+)\s*");
            if (attributeMatch.Success)
            {
                formatAttribute.Name = attributeMatch.Result("${name}");

                int clockRate;
                if (int.TryParse(attributeMatch.Result("${clockrate}"), out clockRate))
                {
                    formatAttribute.ClockRate = clockRate;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Represents a single media format within a media announcement. Often the whole media format can
    /// be represented and described by a single character, e.g. "0" without additional info represents
    /// standard "PCMU", "8" represents "PCMA" etc. For other media types that have variable parameters
    /// additional attributes can be provided.
    /// </summary>
    public class SDPMediaFormat
    {
        /// <summary>
        /// The mandatory ID for the media format. Warning, even though some ID's are normally used to represent
        /// a standard media type, e.g "0" for "PCMU" etc, there is no guarantee that's the case. "0" can be used
        /// for any media format if there is a format attribute describing it. In the absence of a format attribute
        /// then it is required that it represents a standard media type.
        /// 
        /// Note (rj2): FormatID MUST be string (not int), in case ID is 't38' and type is 'image'
        /// <code>
        /// // Example
        /// // Note in this example "0" is representing a standard format so the format attribute is optional.
        /// m=audio 12228 RTP/AVP 0 101         // "0" and "101" are media format ID's.
        /// a=rtpmap:0 PCMU/8000                // "0" is the media format ID.
        /// a=rtpmap:101 telephone-event/8000   // "101" is the media format ID.
        /// a=fmtp:101 0-16
        /// </code>
        /// </summary>
        public string FormatID;

        /// <summary>
        /// The codec in use for this media format.
        /// </summary>
        public SDPMediaFormatsEnum FormatCodec;

        /// <summary>
        /// The optional format attribute for the media format. For standard media types this is not necessary.
        /// <code>
        /// // Example
        /// a=rtpmap:0 PCMU/8000
        /// a=rtpmap:101 telephone-event/8000 // This is the format attribute.
        /// a=fmtp:101 0-16
        /// </code>
        /// </summary>
        public string FormatAttribute { get; set; }

        /// <summary>
        /// The optional format parameter attribute for the media format. For standard media types this is not necessary.
        /// <code>
        /// // Example
        /// a=rtpmap:0 PCMU/8000
        /// a=rtpmap:101 telephone-event/8000 
        /// a=fmtp:101 0-16                     // This is the format parameter attribute.
        /// </code>
        /// </summary>
        public string FormatParameterAttribute { get; set; }

        /// <summary>
        /// The standard name of the media format.
        /// <code>
        /// // Example
        /// a=rtpmap:0 PCMU/8000                // "PCMU" is the media format name.
        /// a=rtpmap:101 telephone-event/8000 
        /// a=fmtp:101 0-16
        /// </code>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The clock rate set on the SDP media format attribute. This will typically not be set for well known media
        /// types and will have a default value of 0.
        /// </summary>
        public int ClockRate { get; set; } = 0;

        /// <summary>
        /// For well known media types this will contain the default clock rate. Warning, if the format is not known or
        /// is dynamic this can be 0.
        /// </summary>
        public int DefaultClockRate { get; set; }

        /// <summary>
        ///  If true this is a standard media format and the attribute line is not required.
        /// </summary>
        public bool IsStandardAttribute { get; set; }

        public SDPMediaFormat(int formatID)
        {
            FormatID = formatID.ToString();
            if (Enum.IsDefined(typeof(SDPMediaFormatsEnum), formatID))
            {
                FormatCodec = (SDPMediaFormatsEnum)Enum.Parse(typeof(SDPMediaFormatsEnum), formatID.ToString(), true);
                Name = FormatCodec.ToString();
                ClockRate = SDPMediaFormatInfo.GetRtpClockRate(FormatCodec);
                //IsStandardAttribute = (formatID < DYNAMIC_ATTRIBUTES_START);
            }
            FormatAttribute = (ClockRate == 0) ? Name : Name + "/" + ClockRate;
        }

        public SDPMediaFormat(string formatID)
        {
            Name = FormatID = formatID;
            FormatCodec = GetFormatCodec(Name);
        }

        public SDPMediaFormat(int formatID, string name) : this(formatID)
        {
            Name = name;
            FormatCodec = GetFormatCodec(Name);
            FormatAttribute = (ClockRate == 0) ? Name : Name + "/" + ClockRate;
        }

        public SDPMediaFormat(int formatID, string name, int clockRate) : this(formatID)
        {
            Name = name;
            FormatCodec = GetFormatCodec(Name);
            ClockRate = clockRate;
            FormatAttribute = (ClockRate == 0) ? Name : Name + "/" + ClockRate;
        }

        public SDPMediaFormat(SDPMediaFormatsEnum format) : this((int)format)
        {
            FormatCodec = format;
        }

        public void SetFormatAttribute(string attribute)
        {
            FormatAttribute = attribute;

            if (SDPFormatAttribute.TryParseFormatAttribute(attribute, out var parsedAttribute))
            {
                Name = parsedAttribute.Name;
                ClockRate = parsedAttribute.ClockRate;
                FormatCodec = GetFormatCodec(Name);
            }  
        }

        public void SetFormatParameterAttribute(string attribute)
        {
            FormatParameterAttribute = attribute;
        }

        /// <summary>
        /// Gets the clock (or sample) rate for this media format.
        /// </summary>
        /// <returns>The clock rate for the media format. Can be 0 if not specified and this is not a 
        /// standard media format.</returns>
        public int GetClockRate()
        {
            if (ClockRate != 0)
            {
                return ClockRate;
            }
            else
            {
                return DefaultClockRate;
            }
        }

        /// <summary>
        /// Attempts to get the codec matching a media format name.
        /// </summary>
        /// <param name="name">The name of the media format to match.</param>
        /// <returns>The media format matching the name. If no match then the unknown format
        /// is returned.</returns>
        public SDPMediaFormatsEnum GetFormatCodec(string name)
        {
            foreach (SDPMediaFormatsEnum format in Enum.GetValues(typeof(SDPMediaFormatsEnum)))
            {
                if (name.ToLower() == format.ToString().ToLower() ||
                    name.Replace('-', '_').ToLower() == format.ToString().ToLower())
                {
                    return format;
                }
            }

            return SDPMediaFormatsEnum.Unknown;
        }

        public override string ToString()
        {
            return FormatAttribute;
        }

        /// <summary>
        /// Attempts to get the compatible formats between two lists. Formats for
        /// "RTP Events" are not included.
        /// </summary>
        /// <param name="a">The first list to match the media formats for.</param>
        /// <param name="b">The second list to match the media formats for.</param>
        /// <returns>A list of media formats that are compatible for BOTH lists.</returns>
        public static List<SDPMediaFormat> GetCompatibleFormats(List<SDPMediaFormat> a, List<SDPMediaFormat> b)
        {
            if (a == null || a.Count == 0)
            {
                throw new ArgumentNullException("a", "The first media format list supplied was empty.");
            }
            else if (b == null || b.Count == 0)
            {
                throw new ArgumentNullException("b", "The second media format list supplied was empty.");
            }

            List<SDPMediaFormat> compatible = new List<SDPMediaFormat>();

            foreach (var format in a.Where(x => x.FormatAttribute == null || !x.FormatAttribute.StartsWith(SDP.TELEPHONE_EVENT_ATTRIBUTE)))
            {
                // TODO: Need to compare all aspects of the format not just the codec.
                if ( b.Any(x => (x.FormatCodec != SDPMediaFormatsEnum.Unknown && x.FormatCodec == format.FormatCodec)
                    || (x.Name != null && format.Name != null && x.Name.ToLower() == format.Name.ToLower())))
                {
                    compatible.Add(format);
                }
            }

            return compatible;
        }
    }
}

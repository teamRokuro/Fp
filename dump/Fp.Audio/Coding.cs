using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1591

namespace Fp.Audio
{
    /* The encoding type specifies the format the sound data itself takes */
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public enum Coding : uint
    {
        coding_SILENCE, /* generates silence */

        /* PCM */
        coding_PCM16LE, /* little endian 16-bit PCM */
        coding_PCM16BE, /* big endian 16-bit PCM */
        coding_PCM16_int, /* 16-bit PCM with sample-level interleave (for blocks) */

        coding_PCM8, /* 8-bit PCM */
        coding_PCM8_int, /* 8-bit PCM with sample-level interleave (for blocks) */
        coding_PCM8_U, /* 8-bit PCM, unsigned (0x80 = 0) */
        coding_PCM8_U_int, /* 8-bit PCM, unsigned (0x80 = 0) with sample-level interleave (for blocks) */
        coding_PCM8_SB, /* 8-bit PCM, sign bit (others are 2's complement) */
        coding_PCM4, /* 4-bit PCM, signed */
        coding_PCM4_U, /* 4-bit PCM, unsigned */

        coding_ULAW, /* 8-bit u-Law (non-linear PCM) */
        coding_ULAW_int, /* 8-bit u-Law (non-linear PCM) with sample-level interleave (for blocks) */
        coding_ALAW, /* 8-bit a-Law (non-linear PCM) */

        coding_PCMFLOAT, /* 32 bit float PCM */

        /* ADPCM */
        coding_CRI_ADX, /* CRI ADX */
        coding_CRI_ADX_fixed, /* CRI ADX, encoding type 2 with fixed coefficients */
        coding_CRI_ADX_exp, /* CRI ADX, encoding type 4 with exponential scale */
        coding_CRI_ADX_enc_8, /* CRI ADX, type 8 encryption (God Hand) */
        coding_CRI_ADX_enc_9, /* CRI ADX, type 9 encryption (PSO2) */

        coding_NGC_DSP, /* Nintendo DSP ADPCM */
        coding_NGC_DSP_subint, /* Nintendo DSP ADPCM with frame subinterframe */
        coding_NGC_DTK, /* Nintendo DTK ADPCM (hardware disc), also called TRK or ADP */
        coding_NGC_AFC, /* Nintendo AFC ADPCM */
        coding_VADPCM, /* Silicon Graphics VADPCM */

        coding_G721, /* CCITT G.721 */

        coding_XA, /* CD-ROM XA */
        coding_PSX, /* Sony PS ADPCM (VAG) */
        coding_PSX_badflags, /* Sony PS ADPCM with custom flag byte */
        coding_PSX_cfg, /* Sony PS ADPCM with configurable frame size (int math) */
        coding_PSX_pivotal, /* Sony PS ADPCM with configurable frame size (float math) */
        coding_HEVAG, /* Sony PSVita ADPCM */

        coding_EA_XA, /* Electronic Arts EA-XA ADPCM v1 (stereo) aka "EA ADPCM" */
        coding_EA_XA_int, /* Electronic Arts EA-XA ADPCM v1 (mono/interleave) */
        coding_EA_XA_V2, /* Electronic Arts EA-XA ADPCM v2 */
        coding_MAXIS_XA, /* Maxis EA-XA ADPCM */
        coding_EA_XAS_V0, /* Electronic Arts EA-XAS ADPCM v0 */
        coding_EA_XAS_V1, /* Electronic Arts EA-XAS ADPCM v1 */

        coding_IMA, /* IMA ADPCM (stereo or mono, low nibble first) */
        coding_IMA_int, /* IMA ADPCM (mono/interleave, low nibble first) */
        coding_DVI_IMA, /* DVI IMA ADPCM (stereo or mono, high nibble first) */
        coding_DVI_IMA_int, /* DVI IMA ADPCM (mono/interleave, high nibble first) */
        coding_3DS_IMA, /* 3DS IMA ADPCM */
        coding_SNDS_IMA, /* Heavy Iron Studios .snds IMA ADPCM */
        coding_OTNS_IMA, /* Omikron The Nomad Soul IMA ADPCM */
        coding_WV6_IMA, /* Gorilla Systems WV6 4-bit IMA ADPCM */
        coding_ALP_IMA, /* High Voltage ALP 4-bit IMA ADPCM */
        coding_FFTA2_IMA, /* Final Fantasy Tactics A2 4-bit IMA ADPCM */
        coding_BLITZ_IMA, /* Blitz Games 4-bit IMA ADPCM */

        coding_MS_IMA, /* Microsoft IMA ADPCM */
        coding_XBOX_IMA, /* XBOX IMA ADPCM */
        coding_XBOX_IMA_mch, /* XBOX IMA ADPCM (multichannel) */
        coding_XBOX_IMA_int, /* XBOX IMA ADPCM (mono/interleave) */
        coding_NDS_IMA, /* IMA ADPCM w/ NDS layout */
        coding_DAT4_IMA, /* Eurocom 'DAT4' IMA ADPCM */
        coding_RAD_IMA, /* Radical IMA ADPCM */
        coding_RAD_IMA_mono, /* Radical IMA ADPCM (mono/interleave) */
        coding_APPLE_IMA4, /* Apple Quicktime IMA4 */
        coding_FSB_IMA, /* FMOD's FSB multichannel IMA ADPCM */
        coding_WWISE_IMA, /* Audiokinetic Wwise IMA ADPCM */
        coding_REF_IMA, /* Reflections IMA ADPCM */
        coding_AWC_IMA, /* Rockstar AWC IMA ADPCM */
        coding_UBI_IMA, /* Ubisoft IMA ADPCM */
        coding_UBI_SCE_IMA, /* Ubisoft SCE IMA ADPCM */
        coding_H4M_IMA, /* H4M IMA ADPCM (stereo or mono, high nibble first) */
        coding_MTF_IMA, /* Capcom MT Framework IMA ADPCM */
        coding_CD_IMA, /* Crystal Dynamics IMA ADPCM */

        coding_MSADPCM, /* Microsoft ADPCM (stereo/mono) */
        coding_MSADPCM_int, /* Microsoft ADPCM (mono) */
        coding_MSADPCM_ck, /* Microsoft ADPCM (Cricket Audio variation) */
        coding_WS, /* Westwood Studios VBR ADPCM */

        coding_AICA, /* Yamaha AICA ADPCM (stereo) */
        coding_AICA_int, /* Yamaha AICA ADPCM (mono/interleave) */
        coding_ASKA, /* Aska ADPCM */
        coding_NXAP, /* NXAP ADPCM */

        coding_TGC, /* Tiger Game.com 4-bit ADPCM */

        coding_NDS_PROCYON, /* Procyon Studio ADPCM */
        coding_L5_555, /* Level-5 0x555 ADPCM */
        coding_LSF, /* lsf ADPCM (Fastlane Street Racing iPhone)*/
        coding_MTAF, /* Konami MTAF ADPCM */
        coding_MTA2, /* Konami MTA2 ADPCM */
        coding_MC3, /* Paradigm MC3 3-bit ADPCM */
        coding_FADPCM, /* FMOD FADPCM 4-bit ADPCM */
        coding_ASF, /* Argonaut ASF 4-bit ADPCM */
        coding_DSA, /* Ocean DSA 4-bit ADPCM */
        coding_XMD, /* Konami XMD 4-bit ADPCM */
        coding_PCFX, /* PC-FX 4-bit ADPCM */
        coding_OKI16, /* OKI 4-bit ADPCM with 16-bit output and modified expand */
        coding_OKI4S, /* OKI 4-bit ADPCM with 16-bit output and cuadruple step */
        coding_PTADPCM, /* Platinum 4-bit ADPCM */
        coding_IMUSE, /* LucasArts iMUSE Variable ADPCM */

        /* others */
        coding_SDX2, /* SDX2 2:1 Squareroot-Delta-Exact compression DPCM */
        coding_SDX2_int, /* SDX2 2:1 Squareroot-Delta-Exact compression with sample-level interleave */
        coding_CBD2, /* CBD2 2:1 Cuberoot-Delta-Exact compression DPCM */
        coding_CBD2_int, /* CBD2 2:1 Cuberoot-Delta-Exact compression, with sample-level interleave */
        coding_SASSC, /* Activision EXAKT SASSC 8-bit DPCM */
        coding_DERF, /* DERF 8-bit DPCM */
        coding_WADY, /* WADY 8-bit DPCM */
        coding_NWA, /* VisualArt's NWA DPCM */
        coding_ACM, /* InterPlay ACM */
        coding_CIRCUS_ADPCM, /* Circus 8-bit ADPCM */
        coding_UBI_ADPCM, /* Ubisoft 4/6-bit ADPCM */

        coding_EA_MT, /* Electronic Arts MicroTalk (linear-predictive speech codec) */
        coding_CIRCUS_VQ, /* Circus VQ */
        coding_RELIC, /* Relic Codec (DCT-based) */
        coding_CRI_HCA, /* CRI High Compression Audio (MDCT-based) */

        coding_OGG_VORBIS, /* Xiph Vorbis with Ogg layer (MDCT-based) */
        coding_VORBIS_custom, /* Xiph Vorbis with custom layer (MDCT-based) */

        coding_MPEG_custom, /* MPEG audio with custom features (MDCT-based) */
        coding_MPEG_ealayer3, /* EALayer3, custom MPEG frames */
        coding_MPEG_layer1, /* MP1 MPEG audio (MDCT-based) */
        coding_MPEG_layer2, /* MP2 MPEG audio (MDCT-based) */
        coding_MPEG_layer3, /* MP3 MPEG audio (MDCT-based) */

        coding_G7221C, /* ITU G.722.1 annex C (Polycom Siren 14) */

        coding_G719, /* ITU G.719 annex B (Polycom Siren 22) */

        //coding_MP4_AAC,         /* AAC (MDCT-based) */

        //coding_AT3plus,         /* Sony ATRAC3plus (MDCT-based) */

        coding_ATRAC9, /* Sony ATRAC9 (MDCT-based) */

        coding_CELT_FSB, /* Custom Xiph CELT (MDCT-based) */

        //coding_SPEEX,           /* Custom Speex (CELP-based) */

        coding_FFmpeg, /* Formats handled by FFmpeg (ATRAC3, XMA, AC3, etc) */
    }
}

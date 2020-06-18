#pragma warning disable 1591
namespace Fp.Intermediate.Model {
    /// <summary>
    /// Defines common humanoid bones
    /// </summary>
    public enum BoneType {
        None = 0x0,

        // Main body
        Hips = 0x100,
        Spine = 0x101,
        Spine1 = 0x102,
        Spine2 = 0x103,
        Neck = 0x104,
        Head = 0x105,

        // Left leg
        LeftUpLeg = 0x200,
        LeftLeg = 0x201,
        LeftFoot = 0x202,
        LeftToeBase = 0x203,

        // Right leg
        RightUpLeg = 0x300,
        RightLeg = 0x301,
        RightFoot = 0x302,
        RightToeBase = 0x303,

        // Left arm
        LeftShoulder = 0x400,
        LeftArm = 0x401,
        LeftForeArm = 0x402,
        LeftHand = 0x403,

        // Right arm
        RightShoulder = 0x500,
        RightArm = 0x501,
        RightForeArm = 0x502,
        RightHand = 0x503,

        // Left hand
        LeftHandThumb1 = 0x600,
        LeftHandThumb2 = 0x601,
        LeftHandThumb3 = 0x602,
        LeftHandThumb4 = 0x603,
        LeftHandIndex1 = 0x610,
        LeftHandIndex2 = 0x611,
        LeftHandIndex3 = 0x612,
        LeftHandIndex4 = 0x613,
        LeftHandMiddle1 = 0x620,
        LeftHandMiddle2 = 0x621,
        LeftHandMiddle3 = 0x622,
        LeftHandMiddle4 = 0x623,
        LeftHandRing1 = 0x630,
        LeftHandRing2 = 0x631,
        LeftHandRing3 = 0x632,
        LeftHandRing4 = 0x633,
        LeftHandPinky1 = 0x640,
        LeftHandPinky2 = 0x641,
        LeftHandPinky3 = 0x642,
        LeftHandPinky4 = 0x643,

        // Right hand
        RightHandThumb1 = 0x700,
        RightHandThumb2 = 0x701,
        RightHandThumb3 = 0x702,
        RightHandThumb4 = 0x703,
        RightHandIndex1 = 0x710,
        RightHandIndex2 = 0x711,
        RightHandIndex3 = 0x712,
        RightHandIndex4 = 0x713,
        RightHandMiddle1 = 0x720,
        RightHandMiddle2 = 0x721,
        RightHandMiddle3 = 0x722,
        RightHandMiddle4 = 0x723,
        RightHandRing1 = 0x730,
        RightHandRing2 = 0x731,
        RightHandRing3 = 0x732,
        RightHandRing4 = 0x733,
        RightHandPinky1 = 0x740,
        RightHandPinky2 = 0x741,
        RightHandPinky3 = 0x742,
        RightHandPinky4 = 0x743
    }
}
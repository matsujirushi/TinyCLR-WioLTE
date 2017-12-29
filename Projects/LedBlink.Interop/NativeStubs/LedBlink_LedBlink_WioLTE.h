#pragma once
namespace LedBlink
{
    struct WioLTE
    {
        // Helper Functions to access fields of managed object
        // Declaration of stubs. These functions are implemented by Interop code developers
        static void LedSetRGB( CLR_RT_HeapBlock* pMngObj, UINT8 param0, UINT8 param1, UINT8 param2, TinyCLR_Result &hr );
    };
}

#include "LedBlink.h"
#include "LedBlink_LedBlink_WioLTE.h"

using namespace LedBlink;


TinyCLR_Result Interop_LedBlink_LedBlink_WioLTE::LedSetRGB___VOID__U1__U1__U1(const TinyCLR_Interop_MethodData md) {
    TINYCLR_HEADER(); hr = S_OK;
    {
        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );

        FAULT_ON_NULL(pMngObj);

        UINT8 param0;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8( stack, 1, param0 ) );

        UINT8 param1;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8( stack, 2, param1 ) );

        UINT8 param2;
        TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8( stack, 3, param2 ) );

        WioLTE::LedSetRGB( pMngObj,  param0, param1, param2, hr );
        TINYCLR_CHECK_HRESULT( hr );
    }
    TINYCLR_NOCLEANUP();
}

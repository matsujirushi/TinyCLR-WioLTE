#include "LedBlink.h"

////////////////////////////////////////////////////////////////////////////////
// CMSIS STM32F4xx Device Peripheral Access Layer Header File.

#define STM32F401xE
#include <inc/stm32f4xx.h>

////////////////////////////////////////////////////////////////////////////////
// STM32F4_Time_Delay() from Targets/STM32F4/STM32F4_Time.cpp

#define STM32F4_AHB_CLOCK_HZ	84000000	// from Devices/WioLTE/Device.h

extern "C" void IDelayLoop(int32_t iterations);

static void STM32F4_Time_Delay(uint64_t nanoseconds)
{
	int32_t iterations = (int32_t)(nanoseconds * STM32F4_AHB_CLOCK_HZ / 1000000000);
	IDelayLoop(iterations - 5);
}

asm volatile (
	".syntax unified\n\t"
	".arch armv7-m\n\t"
	".thumb\n\t"
	".global  IDelayLoop\n\t"
	"@AREA ||i.IDelayLoop||, CODE, READONLY @ void IDelayLoop(UINT32 count)\n\t"
	".section i.IDelayLoop, \"ax\", %progbits\n\t"
	".thumb_func\n\t"
	"IDelayLoop:\n\t"
	"subs    r0, r0, #3          @@ 1 cycle\n\t"
	"bgt     IDelayLoop          @@ 3 cycles taken, 1 cycle not taken.\n\t"
	"bx lr                       @@ 3 cycles\n\t"
	);

////////////////////////////////////////////////////////////////////////////////
// from Targets/STM32F4/STM32F4_GPIO.cpp

// indexed port configuration access
#define Port(port) ((GPIO_TypeDef*) (GPIOA_BASE + (port << 10)))

////////////////////////////////////////////////////////////////////////////////
// Gpio

enum GPIO_DIRECTION_TYPE
{
	GPIO_DIRECTION_INPUT,
	GPIO_DIRECTION_OUTPUT,
};

static void GpioSetDirection(int32_t pin, GPIO_DIRECTION_TYPE direction)
{
	int32_t val;
	switch (direction)
	{
	case GPIO_DIRECTION_INPUT:
		val = 0b00;	// 入力モード
		break;
	case GPIO_DIRECTION_OUTPUT:
		val = 0b01;	// 汎用出力モード
		break;
	default:
		return;
	}

	GPIO_TypeDef* gpio = Port(pin >> 4);
	int32_t gpioNum = pin % 16;

	gpio->MODER = gpio->MODER & ~(0b11 << (gpioNum * 2)) | (val << (gpioNum * 2));
}

static void GpioSetOutput(int32_t pin, bool high)
{
	GPIO_TypeDef* gpio = Port(pin >> 4);
	int32_t gpioNum = pin % 16;

	if (high)
	{
		gpio->BSRR = 1 << gpioNum;			// set bit
	}
	else
	{
		gpio->BSRR = 1 << gpioNum << 16;	// reset bit
	}
}

////////////////////////////////////////////////////////////////////////////////
// RGB LED(SK6812)

static void SK6812Reset(int32_t pin)
{
	GpioSetOutput(pin, false);
	STM32F4_Time_Delay(80000);
}

static void SK6812SetBit(int32_t pin, bool on)
{
	if (!on)
	{
		GpioSetOutput(pin, true);
		STM32F4_Time_Delay(300);
		GpioSetOutput(pin, false);
		STM32F4_Time_Delay(900);
	}
	else
	{
		GpioSetOutput(pin, true);
		STM32F4_Time_Delay(600);
		GpioSetOutput(pin, false);
		STM32F4_Time_Delay(600);
	}
}

static void SK6812SetByte(int32_t pin, uint8_t val)
{
	for (int i = 0; i < 8; i++)
	{
		SK6812SetBit(pin, val & (1 << (7 - i)));
	}
}

static void SK6812SetSingleLED(int32_t pin, uint8_t r, uint8_t g, uint8_t b)
{
	SK6812SetByte(pin, g);
	SK6812SetByte(pin, r);
	SK6812SetByte(pin, b);
}

////////////////////////////////////////////////////////////////////////////////

#define RGB_LED_PWR_PIN	(8)
#define RGB_LED_PIN		(17)

TinyCLR_Result Interop_LedBlink_LedBlink_WioLTE::LedSetRGB___VOID__U1__U1__U1(const TinyCLR_Interop_MethodData md)
{
	auto ip = (const TinyCLR_Interop_Provider*)md.ApiProvider.FindDefault(&md.ApiProvider, TinyCLR_Api_Type::InteropProvider);
	TinyCLR_Interop_ManagedValue r;
	TinyCLR_Interop_ManagedValue g;
	TinyCLR_Interop_ManagedValue b;
	ip->GetArgument(ip, md.Stack, 1, r);
	ip->GetArgument(ip, md.Stack, 2, g);
	ip->GetArgument(ip, md.Stack, 3, b);

	// RGB_LED_PWR_PIN
	GpioSetDirection(RGB_LED_PWR_PIN, GPIO_DIRECTION_OUTPUT);
	GpioSetOutput(RGB_LED_PWR_PIN, true);

	// RGB_LED_PIN
	GpioSetDirection(RGB_LED_PIN, GPIO_DIRECTION_OUTPUT);
	SK6812Reset(RGB_LED_PIN);
	SK6812SetSingleLED(RGB_LED_PIN, r.Data.Numeric->U1, g.Data.Numeric->U1, b.Data.Numeric->U1);

	return TinyCLR_Result::Success;
}

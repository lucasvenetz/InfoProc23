
#include "sys/alt_stdio.h"
#include "system.h"    // Contains definitions of peripheral base addresses
#include "altera_avalon_pio_regs.h"
#include "io.h"
#include <stdio.h>
#include <unistd.h>    // Provides usleep()
#include <stdint.h>
#include <stdbool.h>
#include "altera_avalon_jtag_uart_regs.h"


int main()
{
	alt_u32 keypress;
	alt_32 accel_x, accel_y, accel_z;
	alt_u16 player_health = 10;
	alt_u16 led;
	alt_u16  temp_health;
	int health_valid, final_health;
    while (1)
    {
        // Read the accelerometer data from your custom peripheral.
        // Here, we assume:
        //   Offset 0 holds the filtered X-axis data.
        //   Offset 1 holds the filtered Y-axis data.
        accel_x = IORD(ACCELEROMETER_FILTER_BASE, 0);
        accel_y = IORD(ACCELEROMETER_FILTER_BASE, 1);
        accel_z = IORD(ACCELEROMETER_FILTER_BASE, 2);
        keypress = IORD_ALTERA_AVALON_PIO_DATA(BUTTONS_BASE);
        keypress ^= (0xFFFFFFFF);
        keypress &= (0x00000003);
        alt_printf("%x %x %x %x\n", accel_x, accel_y, accel_z, keypress);
        //alt_printf("%x %x %x \n", accel_x, accel_y, accel_z);
        temp_health = IORD_ALTERA_AVALON_JTAG_UART_DATA(JTAG_UART_BASE);
        health_valid = temp_health >>15;
        if (health_valid == 1) {
        	player_health = temp_health & 0x00FF;
        }
        final_health = player_health - '0';
        led = 0b1111111111 >> (9- final_health);
        IOWR_ALTERA_AVALON_PIO_DATA(LED_BASE,led);

        // Adjust the delay
    }

    return 0;
}


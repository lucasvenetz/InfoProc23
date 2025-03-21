
module CustomNIOS (
	accel_x_writebyteenable_n,
	accel_y_writebyteenable_n,
	accel_z_writebyteenable_n,
	buttons_external_connection_export,
	clk_clk,
	led_external_connection_export,
	reset_reset_n);	

	input	[15:0]	accel_x_writebyteenable_n;
	input	[15:0]	accel_y_writebyteenable_n;
	input	[15:0]	accel_z_writebyteenable_n;
	input	[1:0]	buttons_external_connection_export;
	input		clk_clk;
	output	[9:0]	led_external_connection_export;
	input		reset_reset_n;
endmodule

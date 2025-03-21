module accelerometer_top (
   //////////// CLOCK //////////
   input 		          		ADC_CLK_10,
   input 		          		MAX10_CLK1_50,
   input 		          		MAX10_CLK2_50,

   //////////// SEG7 //////////
   output		     [7:0]		HEX0,
   output		     [7:0]		HEX1,
   output		     [7:0]		HEX2,
   output		     [7:0]		HEX3,
   output		     [7:0]		HEX4,
   output		     [7:0]		HEX5,

   //////////// KEY //////////
   input 		     [1:0]		KEY,

   //////////// LED //////////
   output		     [9:0]		LEDR,

   //////////// SW //////////
   input 		     [9:0]		SW,

   //////////// Accelerometer ports //////////
   output		          		GSENSOR_CS_N,
   input 		     [2:1]		GSENSOR_INT,
   output		          		GSENSOR_SCLK,
   inout 		          		GSENSOR_SDI,
   inout 		          		GSENSOR_SDO
   );

	////// Declarations //////
   localparam SPI_CLK_FREQ  = 2_000_000;  // SPI Clock (Hz)
   localparam UPDATE_FREQ   = 1600;    // Sampling frequency (Hz)

   // clks and reset
   logic rst;
   logic clk, spi_clk, spi_clk_out, locked;

   // output data
   logic data_update;
   logic [15:0] data_x, data_y, data_z;
	logic [15:0] x_filtered, y_filtered, z_filtered;

	/////// PLL instantiation /////////
	pll_inst pll_ip (
		.inclk0 ( MAX10_CLK1_50 ),
		.c0 ( clk ),                 // 25 MHz, phase   0 degrees
		.c1 ( spi_clk ),             //  2 MHz, phase   0 degrees
		.c2 ( spi_clk_out )          //  2 MHz, phase 270 degrees
   );

	/////// SPI Control Instatiation to interact directly with the ADXL345 accelerometer ////////
	spi_main #(     // parameters
      .SPI_CLK_FREQ   (SPI_CLK_FREQ),
      .UPDATE_FREQ    (UPDATE_FREQ))
   spi_controller (      // port connections
      .reset_n    (rst),
      .clk        (clk),
      .spi_clk    (spi_clk),
      .spi_clk_out(spi_clk_out),
      .data_update(data_update),
      .data_x     (data_x),
      .data_y     (data_y),
		.data_z		(data_z),
      .SPI_SDI    (GSENSOR_SDI),
      .SPI_SDO    (GSENSOR_SDO),
      .SPI_CSN    (GSENSOR_CS_N),
      .SPI_CLK    (GSENSOR_SCLK),
      .interrupt  (GSENSOR_INT)
   );
	


	FIR x_data (
		.clk (clk),
		.rst(rst),
		.accelerometer_data(data_x),
		.sample_valid(data_update),
		.filtered_data(x_filtered)
	);

	FIR y_data (
		.clk (clk),
		.rst(rst),
		.accelerometer_data(data_y),
		.sample_valid(data_update),
		.filtered_data(y_filtered)
	);
		
	FIR z_data (
		.clk (clk),
		.rst(rst),
		.accelerometer_data(data_z),
		.sample_valid(data_update),
		.filtered_data(z_filtered)
	);
	
	CustomNIOS u0 (
		.clk_clk                   (clk),                   //     clk.clk
		.reset_reset_n             (rst),             //   reset.reset_n
		.accel_x_writebyteenable_n (x_filtered), // accel_x.writebyteenable_n
		.accel_y_writebyteenable_n (y_filtered),  // accel_y.writebyteenable_n
		.accel_z_writebyteenable_n (z_filtered),
		.buttons_external_connection_export (KEY[1:0]), // buttons_external_connection.export
		.led_external_connection_export     (LEDR[9:0])
	);


////// FPGA On-board outputs ///////////


// Pressing KEY0 freezes the accelerometer's output
assign rst = SW[0];

// 7-segment displays HEX0-3 show data_x in hexadecimal
hex_to_7seg s0 (
   .in      (x_filtered[3:0]),
   .out (HEX0) );

hex_to_7seg s1 (
   .in      (x_filtered[7:4]),
   .out (HEX1) );

hex_to_7seg s2 (
   .in      (x_filtered[11:8]),
   .out (HEX2) );

hex_to_7seg s3 (
   .in      (x_filtered[15:12]),
   .out (HEX3) );
//
//// A few statements just to light some LEDs
//seg7 s4 ( .in(SW[5:2]), .display(HEX4) );
//seg7 s5 ( .in(SW[9:6]), .display(HEX5) );

assign HEX4 = {1'b1, ~x_filtered[15], 6'b111111};

endmodule	

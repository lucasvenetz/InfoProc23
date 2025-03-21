module Sword_Game_top (

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
	
//	//////////// LED //////////
//	output		     [9:0]		LEDR,
//
//	//////////// SW //////////
//	input 		     [9:0]		SW,

	//////////// Accelerometer //////////
	output		          		GSENSOR_CS_N,
	input 		     [2:1]		GSENSOR_INT,
	output		          		GSENSOR_SCLK,
	output 		          		GSENSOR_SDI,
	input 		          		GSENSOR_SDO
	
	
	
);

	////////// XYZ Outputs ///////////
	logic [15:0] x_value, y_value, z_value;
	logic data_valid;
	logic rst;
	logic [5:0]debug;
	logic [15:0] x_filtered, y_filtered, z_filtered;
//Accelerometer_SPI #(
//        .CLK_FREQUENCY(50_000_000),
//        .SPI_FREQUENCY(2_000_000),
//        .UPDATE_FREQUENCY(50),
//        .BYTE_SIZE(8),
//        .IDLE_TIME(200),     // Idle time in nanoseconds
//        .DATA_WIDTH(16)
//) accel_spi_0 (
//	.clk (MAX10_CLK1_50),
//	.rst (rst),
//	.SPI_SDO (GSENSOR_SDO),
//	.SPI_SDI (GSENSOR_SDI),	
//	.SPI_CSN (GSENSOR_CS_N),
//	.SPI_CLK (GSENSOR_SCLK),
//	.interrupt (GSENSOR_INT),
//	.x_value (x_value),
//	.y_value (y_value),
//	.z_value (z_value),
//	.data_valid (data_valid),
//	.a (debug)
//);

	logic clk, spi_clk, spi_clk_out,locked;

pll_inst ppl_ip (
	.areset(rst),
   .inclk0 ( MAX10_CLK1_50 ),
   .c0 ( clk ),                 // 25 MHz, phase   0 degrees
   .c1 ( spi_clk ),             //  2 MHz, phase   0 degrees
   .c2 ( spi_clk_out ),  			//  2 MHz, phase 270 degrees
	.locked(locked)
   );

//===== Instantiation of the spi_control module which provides the logic to 
//      interface to the accelerometer.
spi_control #(     // parameters
      .SPI_CLK_FREQ   (2_000_000),
      .UPDATE_FREQ    (50))
   spi_ctrl (      // port connections
      .reset_n    (rst),
      .clk        (clk),
      .spi_clk    (spi_clk),
      .spi_clk_out(spi_clk_out),
      .data_update(data_valid),
      .data_x     (x_value),
      .data_y     (y_value),
      .SPI_SDI    (GSENSOR_SDI),
      .SPI_SDO    (GSENSOR_SDO),
      .SPI_CSN    (GSENSOR_CS_N),
      .SPI_CLK    (GSENSOR_SCLK),
      .interrupt  (GSENSOR_INT)
   );

FIR x_data (
	.clk (clk),
	.rst(rst),
	.accelerometer_data(x_value),
	.sample_valid(data_valid),
	.filtered_data(x_filtered)
);

	
assign rst = KEY[0];

hex_to_7seg seg0 (
	.in (x_value[3:0]),
	.out (HEX0)
);
hex_to_7seg seg1 (
	.in (x_value[7:4]),
	.out (HEX1)
);
hex_to_7seg seg2 (
	.in (x_value[11:8]),
	.out (HEX2)
);
hex_to_7seg seg3 (
	.in ({1'b0, x_value[14:12]}),
	.out (HEX3)
);

assign HEX4[6] = x_value[15] == 1'b0;



endmodule





`timescale 1ns/1ps

module tb_Sword_Game_top;

    // Declare testbench signals
    logic        ADC_CLK_10;
    logic        MAX10_CLK1_50;
    logic        MAX10_CLK2_50;
    logic [1:0]  KEY;            // Key inputs (active-low reset if needed)
	 logic [9:0]  LEDR;
	 logic [9:0]  SW;
    // Accelerometer signals
    logic  [1:0] GSENSOR_INT;    // Assuming a 2-bit bus for sensor interrupts (inputs)
    logic        GSENSOR_SDO;    // Sensor serial data output (driven by sensor model)
    // The following are outputs from the design:
    logic       GSENSOR_CS_N;
    logic       GSENSOR_SCLK;
    logic       GSENSOR_SDI;
    
    // Seven-seg outputs
    logic [7:0] HEX0, HEX1, HEX2, HEX3, HEX4, HEX5;
    
    // Instantiate the top-level module
    accelerometer_top DUT (
        .ADC_CLK_10  (ADC_CLK_10),
        .MAX10_CLK1_50(MAX10_CLK1_50),
        .MAX10_CLK2_50(MAX10_CLK2_50),
        .HEX0        (HEX0),
        .HEX1        (HEX1),
        .HEX2        (HEX2),
        .HEX3        (HEX3),
        .HEX4        (HEX4),
        .HEX5        (HEX5),
        .KEY         (KEY),
        .GSENSOR_CS_N(GSENSOR_CS_N),
        .GSENSOR_INT (GSENSOR_INT),
        .GSENSOR_SCLK(GSENSOR_SCLK),
        .GSENSOR_SDI (GSENSOR_SDI),
        .GSENSOR_SDO (GSENSOR_SDO)
    );
    
    // Clock generation for MAX10_CLK1_50 (50 MHz => period = 20 ns)
    initial begin
		$display("Simulation Start");
        MAX10_CLK1_50 = 0;
        forever #10 MAX10_CLK1_50 = ~MAX10_CLK1_50;
    
    
    
    // Drive KEY inputs: assume KEY[0] active-low reset; here not pressed (logic '1')
        SW[0] = 1'b1;
    
    // Drive GSENSOR_INT inputs (simulate no interrupt)
        GSENSOR_INT = 2'b11;
    
    
    // Drive GSENSOR_SDO: For a simple test, we tie the sensor output to '0'.
    // You can later modify this to create a more sophisticated sensor model.
        GSENSOR_SDO = 0;
    
    // Simulation duration: run for a period long enough to capture multiple transactions.
        #100000;  // run simulation for 10,000 ns (adjust as needed)
        $finish;
 end

endmodule
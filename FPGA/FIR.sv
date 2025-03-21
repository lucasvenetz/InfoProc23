module FIR #(
	parameter NUM_TAPS = 64,
	parameter A_DATA_WIDTH = 16,
	parameter COEFF_WIDTH = 16,
	parameter PROD_WIDTH = 32,
	parameter OUT_WIDTH = 16
)(

	input logic 							clk,
	input logic 							rst,
	input logic signed [A_DATA_WIDTH - 1:0] accelerometer_data,
	input logic							sample_valid,
	output logic signed [OUT_WIDTH - 1:0]	filtered_data

);

	///////////// Module Parameters ////////////

	localparam logic signed[COEFF_WIDTH-1:0] coefficients [NUM_TAPS:0] = '{			//All FIR coefficients 25 passband 50stopband
   -16'sd1069,  // -0.0326
   -16'sd180,   // -0.0055
   -16'sd187,   // -0.0057
   -16'sd190,   // -0.0058
   -16'sd187,   // -0.0057
   -16'sd173,   // -0.0053
   -16'sd157,   // -0.0048
   -16'sd131,   // -0.0040
   -16'sd98,    // -0.0030
   -16'sd56,    // -0.0017

   -16'sd7,     // -0.0002
    16'sd49,    //  0.0015
    16'sd115,   //  0.0035
    16'sd187,   //  0.0057
    16'sd269,   //  0.0082
    16'sd354,   //  0.0108
    16'sd445,   //  0.0136
    16'sd537,   //  0.0164
    16'sd635,   //  0.0194
    16'sd734,   //  0.0224

    16'sd835,   //  0.0255
    16'sd934,   //  0.0285
    16'sd1028,  //  0.0314
    16'sd1120,  //  0.0342
    16'sd1205,  //  0.0368
    16'sd1287,  //  0.0393
    16'sd1359,  //  0.0415
    16'sd1421,  //  0.0434
    16'sd1475,  //  0.0450
    16'sd1514,  //  0.0462

    16'sd1546,  //  0.0472
    16'sd1564,  //  0.0477
    16'sd1573,  //  0.0480
    16'sd1564,  //  0.0477
    16'sd1546,  //  0.0472
    16'sd1514,  //  0.0462
    16'sd1475,  //  0.0450
    16'sd1421,  //  0.0434
    16'sd1359,  //  0.0415
    16'sd1287,  //  0.0393

    16'sd1205,  //  0.0368
    16'sd1120,  //  0.0342
    16'sd1028,  //  0.0314
    16'sd934,   //  0.0285
    16'sd835,   //  0.0255
    16'sd734,   //  0.0224
    16'sd635,   //  0.0194
    16'sd537,   //  0.0164
    16'sd445,   //  0.0136
    16'sd354,   //  0.0108

    16'sd269,   //  0.0082
    16'sd187,   //  0.0057
    16'sd115,   //  0.0035
    16'sd49,    //  0.0015
   -16'sd7,     // -0.0002
   -16'sd56,    // -0.0017
   -16'sd98,    // -0.0030
   -16'sd131,   // -0.0040
   -16'sd157,   // -0.0048
   -16'sd173,   // -0.0053

   -16'sd187,   // -0.0057
   -16'sd190,   // -0.0058
   -16'sd187,   // -0.0057
   -16'sd180,   // -0.0055
   -16'sd1069   // -0.0326
	};
	
	logic signed [A_DATA_WIDTH-1:0] shiftreg_sample [NUM_TAPS:0];
	logic signed [PROD_WIDTH-1:0] shiftreg_weighted [NUM_TAPS:0];
	logic signed [PROD_WIDTH-1:0] weighted_sum;
	
		////////// INPUT PROCESS //////////
		
	always_ff@(posedge clk or negedge rst) begin
		if (!rst) begin
			
			for (int i =0;i<= NUM_TAPS;i++) begin	//Set all shift regs to 0 (erase all data)
				shiftreg_sample[i] <= 9'b0;
			end
				
			
		end 
		else if (sample_valid) begin
				
			shiftreg_sample[0] <= accelerometer_data;				//Shift reg values for new data
			shiftreg_sample[NUM_TAPS:1] <=shiftreg_sample[NUM_TAPS-1:0];
				
		end
	end
	
		//OUTPUT PROCESS
	
	always_ff@(posedge clk or negedge rst) begin
		if(!rst) begin
		
			filtered_data <= 16'b0;
			
		end 
		else if (sample_valid) begin
		
			filtered_data <= weighted_sum >>> 15;
			
		end
	end
	
	
	genvar j;
	generate
	
		for (j = 0;j<=NUM_TAPS;j++) begin : combine_gen
			assign shiftreg_weighted[j] = shiftreg_sample [j] * coefficients[j];
		end
		
	endgenerate
	
	integer k;
	always_comb begin
		 weighted_sum = 0;
		 for (k = 0; k <= NUM_TAPS; k = k + 1) begin
			  weighted_sum = weighted_sum + shiftreg_weighted[k];
		 end
	end    
	 
endmodule


	
	
	
	
	
				
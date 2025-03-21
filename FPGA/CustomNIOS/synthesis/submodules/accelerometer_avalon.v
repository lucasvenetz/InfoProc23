module accel_peripheral (
   input              clk,
   input              reset_n,
   // Avalon-MM Slave Interface Signals:
   input      [1:0]   address,    
   input              read,
   output reg [31:0]  readdata,
   output             waitrequest,
   input 				 write,
	input		  [31:0]	 writedata,
   // Accelerometer Data Inputs
   input      [15:0]  accel_x,
   input      [15:0]  accel_y,
	input 	  [15:0]	 accel_z
);

   assign waitrequest = 1'b0;   // Read-only setup

   always @(*) begin
      case(address)
         2'b00: readdata = {{16{accel_x[15]}}, accel_x}; 
         2'b01: readdata = {{16{accel_y[15]}}, accel_y};
		   2'b10: readdata = {{16{accel_z[15]}}, accel_z}; 
	
         default: readdata = 32'd0;
      endcase
   end

endmodule

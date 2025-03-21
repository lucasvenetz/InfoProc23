module serdes_control(
        // Host side signals
        input                       reset_n,
        input                       spi_clk,
        input                       spi_clk_out,
        input       [15:0]          data_tx,
        input                       start,
        output                      done,
        output logic  [7:0]           data_rx,
        // SPI side signals
        output                      SPI_SDI,
        input                       SPI_SDO,
        output                      SPI_CSN,
        output                      SPI_CLK
    );


// State Encodings Steps
// 0)Wait for start.
// 1)Write out data.
// 2)Read data.
// 3)Stall for 1 cycle while asserting "done"

typedef enum logic [1:0]{IDLE, WRITE,READ,STALL} state_t;

state_t current_state; 
state_t next_state;
logic [3:0]  count;
logic [15:0] data_tx_reg;
logic read;

logic spi_active;
assign spi_active = (current_state == READ || current_state == WRITE);

// Chip select
assign SPI_CSN = ~(spi_active || start);
// SPI CLK
assign SPI_CLK = spi_active ? spi_clk_out : 1'b1;
// SPI Data. Hold high if not writing.
assign SPI_SDI = (current_state == WRITE) ? data_tx_reg[count] : 1'b1;
// Signal to higher level module that transaction is complete.
assign done    = (current_state == STALL);

always_ff @ (posedge spi_clk or negedge reset_n) begin
	if (reset_n == 1'b0) begin
		current_state <= IDLE;
	end 
	else begin
		current_state <= next_state;
		if (current_state == WRITE) 
			count <= count - 1;			// Decrement event counter
		else if (current_state == READ) begin
			data_rx <= {data_rx[6:0],SPI_SDO};
			count <= count - 1;
		end
		else if (current_state == IDLE) begin
			count <= 4'hf;                    
			data_tx_reg <= data_tx;
			read        <= data_tx[15];

		end
	end
end

always_comb begin
        next_state = current_state;
		  case (current_state)
		  
            IDLE : begin
                if (start) begin
                    next_state  = WRITE;
                end
            end

            WRITE : begin
                // If reading and we're writing the last bit
                // Stay here if performing a write. Otherwise, branch to the
                // READ state.
                if (read && (count == 8)) begin
                    next_state = READ;
                end else if (count == 0) begin
                    next_state = STALL;
                end
            end

            READ : begin
                // Shift in data
                if (count == 0) begin
                    next_state = STALL;
                end
            end
            // One clock cycle idle state.
            STALL: next_state = IDLE;
				default: next_state = IDLE;
        endcase
	end
endmodule

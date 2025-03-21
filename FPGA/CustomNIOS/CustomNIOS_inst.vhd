	component CustomNIOS is
		port (
			accel_x_writebyteenable_n          : in  std_logic_vector(15 downto 0) := (others => 'X'); -- writebyteenable_n
			accel_y_writebyteenable_n          : in  std_logic_vector(15 downto 0) := (others => 'X'); -- writebyteenable_n
			accel_z_writebyteenable_n          : in  std_logic_vector(15 downto 0) := (others => 'X'); -- writebyteenable_n
			buttons_external_connection_export : in  std_logic_vector(1 downto 0)  := (others => 'X'); -- export
			clk_clk                            : in  std_logic                     := 'X';             -- clk
			led_external_connection_export     : out std_logic_vector(9 downto 0);                     -- export
			reset_reset_n                      : in  std_logic                     := 'X'              -- reset_n
		);
	end component CustomNIOS;

	u0 : component CustomNIOS
		port map (
			accel_x_writebyteenable_n          => CONNECTED_TO_accel_x_writebyteenable_n,          --                     accel_x.writebyteenable_n
			accel_y_writebyteenable_n          => CONNECTED_TO_accel_y_writebyteenable_n,          --                     accel_y.writebyteenable_n
			accel_z_writebyteenable_n          => CONNECTED_TO_accel_z_writebyteenable_n,          --                     accel_z.writebyteenable_n
			buttons_external_connection_export => CONNECTED_TO_buttons_external_connection_export, -- buttons_external_connection.export
			clk_clk                            => CONNECTED_TO_clk_clk,                            --                         clk.clk
			led_external_connection_export     => CONNECTED_TO_led_external_connection_export,     --     led_external_connection.export
			reset_reset_n                      => CONNECTED_TO_reset_reset_n                       --                       reset.reset_n
		);


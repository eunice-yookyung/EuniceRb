%% 2024/02/05 - Plot the traces of the different channels, includes array capabilities
% 2024/10/28 - fixes issue with digitial overlap interpolating
% 2024/12/19 - fixed minor issue handling duplicate time values in the digital flag if statement

clear
close all

path_files = '';
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
sequence_names = {'chiral_anyon_ramp_full_return', 'anyons_ramp_upgrade_v2'};
batch_lines = [2,2];
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
N_seq = numel(sequence_names);

% Where all the functions are defined
addpath('W:\Timeline\Subs\') 


%% Some functions

% Use this function to extract the value of a variable base on its name, e.g.
% i = findIndex(variable_list, 'name_variable');
% value_variable = eval(variable_list{i}{2});
firstCell = @(x) x{1};
findIndex = @(list, element) find(strcmp(cellfun(firstCell, list, 'UniformOutput', false), element));

line_alpha = 0.4;
color_list = [[0, 0.4470, 0.7410, line_alpha]; [0.8500, 0.3250, 0.0980, line_alpha]; ...
    [0.4660, 0.6740, 0.1880, line_alpha]; [0.9290, 0.6940, 0.1250, line_alpha]; ...
    [0.4940, 0.1840, 0.5560, line_alpha]];
    

%% Set up some loop stuff

% Import the different channels' names
ChannelsWithCard = import_ChannelsWithCard([sequence_names{1} '\']);
N_ChannelsWithCard = numel(ChannelsWithCard);

%%% Set channels to plot
% MOT channels full
% channel_list = {'mot_low_current', 'ta_shutter', 'repump_shutter', ...
%     'mot_detuning', 'mot_high_current', 'cap_discharge', ...
%     'ttl_80mhz', 'ttl_97mhz', 'ttl_n133mhz', 'ttl_n78mhz', 'ttl_78mhz', ...
%     'optical_pumping', 'polarizer_shutter_11'}; 
%
% % MOT channels inverted
% channel_list = {'mot_low_current', 'ta_shutter', 'repump_shutter'}; 
%
% MOT channels inverted + DMD tracking
% channel_list = {'mot_low_current', 'ta_shutter', 'repump_shutter', 'apogee_camera', ...
%     'line_dmd_ttl', 'hor_dmd_ttl', 'lattice2d765_ttl', 'lattice2d765_shutter', 'ixon_flip_mount_ttl'}; 
% 
% % DMD tracking
% channel_list = {'mot_low_current', 'apogee_camera', 'line_dmd_ttl', 'hor_dmd_ttl', 'lattice2d765_ttl', 'lattice2d765_shutter', 'ixon_flip_mount_ttl'}; 
%
% FQH
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'line_dmd_power', ...
%     'hor_dmd_power', 'ps6_ao', 'ps8_ao', 'gauge1_power', 'gauge2_power', 'gauge2_rf_fm', 'gauge_ttl', 'gauge_shutter'};
%
% E and U cal
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'axial_lattice_power', ...
%     'red_dipole_power', 'ps1_ao', 'ps5_ao', 'ps6_ao', 'ps8_ao'};
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'axial_lattice_power',...
%     'ps1_ao', 'ps5_ao', 'ps6_ao', 'ps8_ao', 'transport_13', ...
%     'red_dipole_power', 'anticonfin_ttl', 'anticonfin_shutter', ...
%     'imaging_coil', 'offset_fet'};
% Lattice power and shutters
% channel_list = {'lattice2d765_power', 'lattice2d765_ttl', 'lattice2d765_shutter', ...
%     'lattice2d765_power2', 'lattice2d765_ttl2', 'lattice2d765_shutter2', ...
%     'axial_lattice_power', 'ttl_axial_lattice', 'axial_lattice_shutter', ...
%     'red_dipole_power', 'anticonfin_ttl', 'anticonfin_shutter'};
% all PS with mosfet
% channel_list = {'ps1_ao', 'single_quad_shim', 'quad_shim', 'quad_fet', 'ps1_shunt', 'quic_fet', ...
%     'ps2_ao', 'offset_fet', 'bias_siphon', ...
%     'ps3_ao', 'ps4_ao', 'ps4_shunt', 'ps7_ao', 'imaging_coil', ...
%     'ps5_ao', 'ps5_enable', 'ps5_shunt', ...
%     'ps6_ao', 'ioffe_mirror_fet', 'bias_enable', ...
%     'ps8_ao', 'quad_shim2', 'ps8_shunt'};
% channel_list = {'ps1_ao', 'single_quad_shim', 'quad_shim', ...
%     'ps2_ao', 'bias_siphon', 'offset_fet', ...
%     'ps5_ao', 'ps5_enable', 'ps5_shunt', ...
%     'ps6_ao', 'ioffe_mirror_fet', 'bias_enable', ...
%     'ps8_ao', 'quad_shim2', 'ps8_shunt'};
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', ...
%     'ps1_ao', 'single_quad_shim', 'quad_shim', ...
%     'ps5_ao', 'ps5_enable', ...
%     'ps6_ao', 'offset_fet', 'bias_enable',...
%     'ps8_ao', 'quad_shim2',};

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% channel_list = {'lattice2d765_power', 'lattice2d765_power2', 'lattice2d795_power2', ...
%     'line_dmd_power', 'line_dmd_trigger', 'hor_dmd_power', 'hor_dmd_trigger', ...
%     'ps5_ao', 'ps6_ao'};  
channel_list = {'lattice2d765_power', 'lattice2d765_power2', ...
    'line_dmd_power', 'hor_dmd_power', 'ps5_ao'};  
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
N_chan = numel(channel_list);

% initialize lists
time_list = cell(N_seq,N_chan);
values_list = cell(N_seq,N_chan);

t_start_all = [];
t_stop_all = [];
variable_list = {};

for seq_idx = 1:N_seq
   
    %% Import and read sequence line-by-line
    name_sequence = [sequence_names{seq_idx} '.vb'];
    batch_line = batch_lines(seq_idx);
    path_files = [sequence_names{seq_idx} '\'];
    
    disp(['Sequence: ' name_sequence])
    disp(['  batch line: ' num2str(batch_line)])

    [instruction_list, arguments_list, variable_list{seq_idx}, sub_variable_containers, ExpConstants, LogExpParam] ...
        = read_Sequence(path_files, name_sequence, batch_line);
    N_inst = numel(instruction_list);
    

    %% Assign instruction to each channel
  
    % Object to contain instructions per channel, labelled by channel
    channel_instruction = containers.Map;
    channel_instruction_bare = containers.Map;
    for j = 1:N_ChannelsWithCard
        channel_instruction(ChannelsWithCard{j}) = {};
        channel_instruction_bare(ChannelsWithCard{j}) = {};
    end

    for i = 1:N_inst
        [time_aux, values_aux] = instruction_Into_Points(arguments_list{i});
        t_start_all = [t_start_all, min(time_aux)];
        t_stop_all = [t_stop_all, max(time_aux)];
        channel = return_Channel(instruction_list{i}, arguments_list{i});
        channel_instruction(channel) = [channel_instruction(channel), {arguments_list{i}}];
        channel_instruction_bare(channel) = [channel_instruction_bare(channel), {instruction_list{i}}];
    end


    %% Read instructions into final values (time_list and values_list)

    for k = 1:N_chan         
        channel = channel_list{k};
        list_instructions_bare = channel_instruction_bare(channel);
        list_instructions = channel_instruction(channel);
        N_inst_2 = numel(list_instructions);
    
        % Initialization for the time-span variables
        t_start = 0;
        t_stop = 0;
        
        % These will contain the time evolution
        time = [];
        values = [];
        
        for j = 1:N_inst_2
            overwrite_flag = 0;
            digital_flag = 0;
            time_end_seg = [];
            [time_aux, values_aux] = instruction_Into_Points(list_instructions{j});

            ramp_type_split = split(list_instructions_bare(j),'.');
            ramp_type = ramp_type_split{1};
            if contains(ramp_type,'digitaldata')
                digital_flag = 1;
            end

            if numel(time_aux) > 0
                t_start = min(time_aux);
                t_stop_aux = max(time_aux);                
                if time_aux(1) > time_aux(end)
                    disp("Do nothing")                
                elseif t_start > t_stop % If the new timestep is not stuck to the previous one
                    time = [time, t_stop + 10^(-6), t_start - 10^(-6)]; % Put something in between, the 1 nanosecond should not be visible
                    values = [values, 0, 0];                
                elseif t_stop > t_start
                    overwrite_flag = 1;
                    disp('Previous step finishes after the new one...')
                    disp(list_instructions_bare(j))
                    disp(list_instructions{j})

                    if digital_flag
                        disp('WARNING: digital overwrite!!!!')
                        disp(' ')
                        inter_values_idx = (time >= t_start) & (time <= t_stop_aux);
                        old_values_inter = values(inter_values_idx);
                        all_times = sort([time(inter_values_idx), time_aux]);
                        all_times = unique(all_times);
                        new_times_idx = ismember(all_times, time_aux);
                        new_idx = find(new_times_idx);
                        old_times_idx = ismember(all_times, time(inter_values_idx));
    
                        old_values_full = zeros(size(all_times));
                        new_values_full = zeros(size(all_times)) + values_aux(1);
                        old_values_full(old_times_idx) = old_values_inter;                    
                        for ii = 1:numel(time_aux)
                            [closest_val, idx_closest_val] = min(abs(time-time_aux(ii)));
                            old_values_full(new_idx(ii)) = values(idx_closest_val);
                        end
                        values_aux = 5 * xor(old_values_full, new_values_full);
                        time_aux = all_times;
                    end

                    good_index = time < t_start;
                    good_index2 = time > t_stop_aux;
                    time_end_seg = time(good_index2);
                    values_end_seg = values(good_index2);
                    time = [time(good_index), t_start];
                    values = values(good_index);
                    values = [values, values(end)];
                end

                if ~(time_aux(1) > time_aux(end))
                    time = [time, time_aux];
                    values = [values, values_aux];                    
                    if overwrite_flag && (numel(time_end_seg) > 0)
                        if time_end_seg(1) > time(end)
                            time = [time, time(end) + 10^(-6), time_end_seg(1) - 10^(-6)]; % Put something in between, the 1 nanosecond should not be visible
                            if digital_flag
                                values = [values, values_end_seg(1), values_end_seg(1)];
                            else
                                values = [values, 0, 0];
                            end
                        end
                        time = [time,time_end_seg];
                        values = [values, values_end_seg];
                    end
                    t_stop = max(time);
                else
                    disp('ERROR: reversed times')
                end

            end
        end

        time_list{seq_idx, k} = time;
        values_list{seq_idx, k} = values;    
    end
end


%% Look at the values of some variables (use lower letters)

for j = 1:N_seq
    % variables involved in nearly every sequence:  

    % i = findIndex(variable_list{j}, 'mot_end_time');
    % mot_end_time{j} = eval(variable_list{j}{i}{2}); % in ms
    % 
    % i = findIndex(variable_list{j}, 'transport_start_time');
    % transport_start_time = eval(variable_list{j}{i}{2}); % in ms
    % i = findIndex(variable_list{j}, 'transport_end_time');
    % transport_end_time = eval(variable_list{j}{i}{2}); % in ms
    % 
    % i = findIndex(variable_list{j}, 'evaporation_end_time');
    % evaporation_end_time{j} = eval(variable_list{j}{i}{2}); % in ms
    % 
    i = findIndex(variable_list{j}, 'twodphysics_start_time');
    twodphysics_start_time{j} = eval(variable_list{j}{i}{2}); % in ms
    i = findIndex(variable_list{j}, 'twodphysics_end_time');
    twodphysics_end_time{j} = eval(variable_list{j}{i}{2}); % in ms
    
    i = findIndex(variable_list{j}, 'pinning_start_time');
    pinning_start_time{j} = eval(variable_list{j}{i}{2}); % in ms
    i = findIndex(variable_list{j}, 'pinning_end_time');
    pinning_end_time{j} = eval(variable_list{j}{i}{2}); % in ms
    
    % i = findIndex(variable_list{j}, 'last_time');
    % last_time{j} = eval(variable_list{j}{i}{2}); % in ms   
   
    % variables unique to this sequence: 
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    i = findIndex(variable_list{j}, 'mod_start_time');
    mod_start_time{j} = eval(variable_list{j}{i}{2});
    i = findIndex(variable_list{j}, 'mod_end_time');
    mod_end_time{j} = eval(variable_list{j}{i}{2});

    i = findIndex(variable_list{j}, 'hold_start_time');
    hold_start_time{j} = eval(variable_list{j}{i}{2});
    i = findIndex(variable_list{j}, 'hold_end_time');
    hold_end_time{j} = eval(variable_list{j}{i}{2});

    % i = findIndex(variable_list{j}, 'line_load_end_time');
    % line_load_end_time{j} = eval(variable_list{j}{i}{2}); % in ms
    % 
    % i = findIndex(variable_list{j}, 'lattice2_freeze_start_time');
    % lattice2_freeze_start_time{j} = eval(variable_list{j}{i}{2}); % in ms
    % i = findIndex(variable_list{j}, 'lattice2_freeze_end_time');
    % lattice2_freeze_end_time{j} = eval(variable_list{j}{i}{2}); % in ms
    % 
    % i = findIndex(variable_list{j}, 'lattice1_freeze_start_time');
    % lattice1_freeze_start_time{j} = eval(variable_list{j}{i}{2}); % in ms
    % i = findIndex(variable_list{j}, 'lattice1_freeze_end_time');
    % lattice1_freeze_end_time{j} = eval(variable_list{j}{i}{2}); % in ms
    %
    % i = findIndex(variable_list{j}, 'berlin_wall_turnoff_start_time');
    % berlin_wall_turnoff_start_time{j} = eval(variable_list{j}{i}{2}); % in ms
    % i = findIndex(variable_list{j}, 'berlin_wall_turnoff_end_time');
    % berlin_wall_turnoff_end_time{j} = eval(variable_list{j}{i}{2}); % in ms
    %
    % i = findIndex(variable_list{j}, 'full_counting_start_time');
    % full_counting_start_time = eval(variable_list{j}{i}{2}); % in ms
    % i = findIndex(variable_list{j}, 'full_counting_end_time');
    % full_counting_end_time = eval(variable_list{j}{i}{2}); % in ms
    
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
end 

%%% Variables in specific sequence
% i = findIndex(variable_list{2}, 'pinning_start_time');
% pinning_start_time{2} = eval(variable_list{2}{i}{2}); % in ms
% i = findIndex(variable_list{2}, 'pinning_end_time');
% pinning_end_time{2} = eval(variable_list{2}{i}{2}); % in ms
% 
% i = findIndex(variable_list{2}, 'mod_start_time');
% mod_start_time{2} = eval(variable_list{2}{i}{2}); % in ms
% i = findIndex(variable_list{2}, 'mod_end_time');
% mod_end_time{2} = eval(variable_list{2}{i}{2}); % in ms

%%% If it's in a subsequence
% keys_containers = sub_variable_containers.keys;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% sub_variable_list = sub_variable_containers(keys_containers{3});
% i = findIndex(sub_variable_list, 'twod_start_time');
% twod_start_time = eval(sub_variable_list{i}{2}); % in ms
%
% sub_variable_list = sub_variable_containers(keys_containers{3});
% i = findIndex(sub_variable_list, 'dimple_start_time');
% dimple_start_time = eval(variable_list{i}{2}); % in ms
%
% sub_variable_list = sub_variable_containers(keys_containers{3});
% i = findIndex(sub_variable_list, 'dimple_ready_time');
% dimple_ready_time = eval(variable_list{i}{2}); % in ms
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

%%% if it's in exp constants
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
i = findIndex(ExpConstants, 'mot_load_time');
% mot_load_time = eval(ExpConstants{i}{2}); % in ms
% 
% i = findIndex(ExpConstants, 'molasses_time');
% molasses_time = eval(ExpConstants{i}{2}); % in ms
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%


%% Multiple traces at the same time (use lower letters for the channels)

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Time window to look at
% t_start_plot = min(t_start_all);
t_start_plot = min([twodphysics_start_time{:}]) - 10;

% t_stop_plot = max(t_stop_all);
t_stop_plot = max([pinning_start_time{:}]) + 50;
% t_stop_plot = max([twodphysics_end_time{:}]) + 10;

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
for seq_idx = 1:N_seq 
    for k = 1:N_chan    
        time = time_list{seq_idx, k};
        values = values_list{seq_idx, k};
        if numel(time)>0
            t_stop = time(end);            
        else
            t_stop = 0;
        end
        if t_stop < t_stop_plot
            time = [time, t_stop + 10^(-6), t_stop_plot - 10^(-6)]; 
            values = [values, 0, 0];
        end
        time_list{seq_idx, k} = time;
        values_list{seq_idx, k} = values;  
    end
end


%% Plot figure

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
plot_figure = 1;
save_figure = 1;
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

if plot_figure
    clear ax
    N_chan_tot = N_chan+1;
    figure('Units','normalized', 'OuterPosition', [0.26, 0.03, 0.5, 0.97])
    tl = tiledlayout(N_chan_tot, 1, 'TileSpacing', 'tight', 'Padding', 'tight');   
   
    for k = 1:N_chan
        ax(k) = nexttile;
        hold on
        for j = 1:N_seq
            plot(ax(k), time_list{j, k} ./ 1000, values_list{j, k}, 'Linewidth', 1.5, ...
                'DisplayName', sequence_names{j}, 'Color', color_list(j,:));
        end
        xlim([t_start_plot / 1000, t_stop_plot / 1000])
        ylabel(replace(channel_list{k}, '_', ' '))
         if ~(k==N_chan)
            xticklabels([])
        end
    end
    ax(N_chan_tot) = nexttile;       
    
    for k = 1:N_chan_tot   
        clear xl
        hold on

        %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% 
        
        % sequence 1
        sidx = 1;    
        xl(1) = xline(ax(k), twodphysics_start_time{sidx}/1000, '-.', '2D phys start', ...
            'Linewidth', 1.5, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), twodphysics_end_time{sidx}/1000, '-.', '2D phys end', ...
            'Linewidth', 1.5, 'Color', color_list(sidx,:));
        % xl(end+1) = xline(ax(k), berlin_wall_turnoff_start_time{sidx}/1000, '--', 'wall turnoff start', ...
        %     'Linewidth', 1.5, 'Color', color_list(sidx,:));
        % xl(end+1) = xline(ax(k), berlin_wall_turnoff_end_time{sidx}/1000, '--', 'wall turnoff end', ...
        %     'Linewidth', 1.5, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), mod_start_time{sidx}/1000, '--', 'mod start', ...
            'Linewidth', 1.5, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), mod_end_time{sidx}/1000, '--', 'mod end', ...
            'Linewidth', 1.5, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), hold_start_time{sidx}/1000, ':', 'hold start', ...
            'Linewidth', 1.25, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), hold_end_time{sidx}/1000, ':', 'hold end', ...
            'Linewidth', 1.25, 'Color', color_list(sidx,:));

        % sequence 2 
        sidx = 2;
        xl(end+1) = xline(ax(k), twodphysics_start_time{sidx}/1000, '-.', '2D phys start', ...
            'Linewidth', 1.5, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), twodphysics_end_time{sidx}/1000, '-.', '2D phys end', ...
            'Linewidth', 1.5, 'Color', color_list(sidx,:));
        % xl(end+1) = xline(ax(k), berlin_wall_turnoff_start_time{sidx}/1000, '--', 'wall turnoff start', ...
        %     'Linewidth', 1.5, 'Color', color_list(sidx,:));
        % xl(end+1) = xline(ax(k), berlin_wall_turnoff_end_time{sidx}/1000, '--', 'wall turnoff end', ...
        %     'Linewidth', 1.5, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), mod_start_time{sidx}/1000, '--', 'mod start', ...
            'Linewidth', 1.5, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), mod_end_time{sidx}/1000, '--', 'mod end', ...
            'Linewidth', 1.5, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), hold_start_time{sidx}/1000, ':', 'hold start', ...
            'Linewidth', 1.25, 'Color', color_list(sidx,:));
        xl(end+1) = xline(ax(k), hold_end_time{sidx}/1000, ':', 'hold end', ...
            'Linewidth', 1.25, 'Color', color_list(sidx,:));
       
        %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

        if ~(k == N_chan_tot)
            for xli = xl
                xli.Annotation.LegendInformation.IconDisplayStyle = "off";
                xli.Label = ' ';
            end
        else
            for xli = xl
                xli.Annotation.LegendInformation.IconDisplayStyle = "off";
                xli.FontSize = 7;
                xli.Interpreter = 'none';
            end
        end
    end

    for xli = xl
        xli.LabelHorizontalAlignment = 'center';
    end   
    xlim([t_start_plot / 1000, t_stop_plot / 1000]) 
    yticklabels([])
    yticks([])
    xticklabels([])
    xticks([])

    xlabel(ax(end), 'Time (s)')
    title(tl, replace(name_sequence,'_', '\_'))
    linkaxes(ax, 'x')
    legend(ax(end-1),'Location','eastoutside', 'Interpreter','none')
    hold off

    if save_figure
        print(['timeline_plots_combined'],'-dpng')
    end

end

function [] = plotParticipantAverageOverTime(difference_table)
% 
% Per participant average joint over time
% 

joints_util;
plot_colors;

kinect_configs = {
    'Parallel', '45-Degrees-apart', '90-Degrees-apart'
};

scenarios = {
    'Stationary', 'Steps', 'Walk'
};

title_format = 'Average Distances over Time in %s Scenario with %s Kinects';
dir = 'Plots/Average_Distance_over_Time/';
filename_format = strcat(dir,'Average_Distance_over_Time_ScenarioId_%s_KinectConfig_%s');

first_avg_dx = 6;
first_avg_dy = 7;
first_avg_dz = 8;
first_avg_dd = 9;
last_idx = 5+length(joint_types)*4;

participant_id = 0;

s_table = difference_table(difference_table.Study_Id==participant_id,:);

for kinect_config = unique(s_table.Kinect_Config,'rows').'
    k_table = s_table(s_table.Kinect_Config==kinect_config,:);

    for scenario_id = unique(k_table.Scenario_Id,'rows').'
        scen_table = k_table(k_table.Scenario_Id==scenario_id & k_table.Person_Id==0,:);

        if (scenario_id ~= 1 && scenario_id~=2 && scenario_id~=3)
            continue;
        end
        
        timestamps_x = scen_table{:,'Tracker_Time'};

        avg_dx = zeros(length(timestamps_x),1);
        std_dx = zeros(length(timestamps_x),1);
        avg_dy = zeros(length(timestamps_x),1);
        std_dy = zeros(length(timestamps_x),1);
        avg_dz = zeros(length(timestamps_x),1);
        std_dz = zeros(length(timestamps_x),1);
        avg_dd = zeros(length(timestamps_x),1);
        std_dd = zeros(length(timestamps_x),1);

        row_counter = 1;
        for t = unique(scen_table.Tracker_Time,'rows').'
            t_table = scen_table(scen_table.Tracker_Time==t,:);
            avg_dx(row_counter,1) = mean(t_table{1,first_avg_dx:8:last_idx},2);
            std_dx(row_counter,1) = std(t_table{1,first_avg_dx:8:last_idx},0,2);
            avg_dy(row_counter,1) = mean(t_table{1,first_avg_dy:8:last_idx},2);
            std_dy(row_counter,1) = std(t_table{1,first_avg_dy:8:last_idx},0,2);
            avg_dz(row_counter,1) = mean(t_table{1,first_avg_dz:8:last_idx},2);
            std_dz(row_counter,1) = std(t_table{1,first_avg_dz:8:last_idx},0,2);
            avg_dd(row_counter,1) = mean(t_table{1,first_avg_dd:8:last_idx},2);
            std_dd(row_counter,1) = std(t_table{1,first_avg_dd:8:last_idx},0,2);
            row_counter = row_counter+1;
        end

        figure;
        hold on;
        x_h = shadedErrorBar(timestamps_x,avg_dx,std_dx,'-r',1);
        y_h = shadedErrorBar(timestamps_x,avg_dy,std_dy,'-g',1);
        z_h = shadedErrorBar(timestamps_x,avg_dz,std_dz,'-b',1);
        d_h = shadedErrorBar(timestamps_x,avg_dd,std_dd,'-k',1);
        box on;
        hold off;

        plot_title = sprintf(title_format, scenarios{1,scenario_id}, kinect_configs{1,kinect_config+1});
        plot_filename = sprintf(filename_format, scenarios{1,scenario_id}, kinect_configs{1,kinect_config+1});

        title(plot_title,'Fontsize',15);
        xlabel('Time (s)','Fontsize',15);
        ylabel('Distance (cm)','Fontsize',15);
        legend([x_h.mainLine,y_h.mainLine,z_h.mainLine,d_h.mainLine],'Avg. \Delta x','Avg. \Delta y','Avg. \Delta z','Avg. \Delta d','Location','northwest');

        set(gcf,'Visible','Off');
        set(gcf,'PaperPositionMode','Manual');
        set(gcf,'PaperUnits','Normalized');
        print('-dsvg','-painters',plot_filename);

    end
end
end


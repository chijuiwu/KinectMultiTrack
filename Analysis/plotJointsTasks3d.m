function [] = plotJointsTasks3d(joints_average_study_table)
%PLOTJOINTSTASKS3D Summary of this function goes here
%   Detailed explanation goes here

joints_util;
plot_colors;

% dd, tasks, joint types
title_format = 'XXXXX';
dir = 'Plots/Joints_over_Time/';
filename_format = strcat(dir,'XXXX');

first_avg_dd = 10;
last_idx = 3+length(joint_types)*8;

for participant_id = unique(difference_table.Study_Id,'rows').'
    s_table = difference_table(difference_table.Study_Id==participant_id,:);
    
    for kinect_config = unique(s_table.Kinect_Config,'rows').'
        k_table = s_table(s_table.Kinect_Config==kinect_config,:);
        
        for scenario_id = unique(k_table.Scenario_Id,'rows').'
            scen_table = k_table(k_table.Scenario_Id==scenario_id,:);

            try
                fprintf('Participant: %d, Kinect_Config: %d, Scenario_Id: %d\n', participant_id, kinect_config, scenario_id);

                timestamps_x = scen_table{:,'Tracker_Time'};
                x = repmat(timestamps_x,1,length(joint_types))';
                x = x(:);
                joint_types_y = (1:length(joint_types))';
                y = repmat(joint_types_y,1,length(timestamps_x));
                y = y(:);

                dd_avg = zeros(length(timestamps_x)*length(joint_types),1);
                row_counter = 1;
                for t = unique(scen_table.Tracker_Time,'rows').'
                    t_joints_dd = scen_table{scen_table.Tracker_Time==t,first_avg_dd:4:last_idx}';
                    dd_avg(row_counter:row_counter+length(joint_types)-1,1) = t_joints_dd;
                    row_counter = row_counter+length(joint_types);
                end

                figure;
                hold on;
                scatter3(x,y,dd_avg,[],y,'filled');
                colormap(jet);
                view(45,30);
                box on;
                hold off;

                plot_title = sprintf(title_format);
                plot_filename = sprintf(filename_format);

                title(plot_title,'Fontsize',15);
                xlabel('Timestamp (sec)','Fontsize',15);
                ylabel({'','Joint Types'},'Fontsize',15);
                zlabel('\Delta Distance (cm)','Fontsize',15);
                set(gca,'YLim',[0.5 length(joint_types)+0.5]);
                set(gca,'YTick',1:length(joint_types),'YTickLabel',joint_types);
                ax = gca;
                ax.YTickLabelRotation = -45;

                set(gcf,'Visible','Off');
                set(gcf,'PaperPositionMode','Manual');
                set(gcf,'PaperUnits','Normalized');
                print('-dsvg','-painters',plot_filename);
            catch
            end
        end
    end
end


end


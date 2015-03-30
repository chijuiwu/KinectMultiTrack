function [] = plotParticipantJointsOverTime(difference_table)
% 
% Per participant joints over time
% 

joints_util;
plot_colors;

title_format = 'Joints over Time - [Participant: %d; KinectConfig: %d; ScenarioId: %d]';
dir = 'Plots/Joints_over_Time/';
filename_format = strcat(dir,'Joints_over_Time_Participant_%d_KinectConfig_%d_ScenarioId_%d');

first_avg_dx = 6;
first_avg_dy = 7;
first_avg_dz = 7;
first_avg_dd = 9;
last_idx = 5+length(joint_types)*4;

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

                plot_title = sprintf(title_format, participant_id, kinect_config, scenario_id);
                plot_filename = sprintf(filename_format, participant_id, kinect_config, scenario_id);

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
function [] = plotParticipantJointsAverage(time_average_table)
% 
% Per participant joints average
% showing dx, dy, dz, dd for all joints
% 

joints_util;
plot_colors;

title_format = 'Joints Average - [Participant: %d; KinectConfig: %d; ScenarioId: %d]';
dir = 'Plots/Joints_Average/';
filename_format = strcat(dir,'Joints_Average_Participant_%d_KinectConfig_%d_ScenarioId_%d');

first_avg_dx = 5;
first_avg_dy = 7;
first_avg_dz = 9;
first_avg_dd = 11;
last_idx = 4+length(joint_types)*8;

for participant_id = unique(time_average_table.Study_Id,'rows').'
    s_table = time_average_table(time_average_table.Study_Id==participant_id,:);
    
    for kinect_config = unique(s_table.Kinect_Config,'rows').'
        k_table = s_table(s_table.Kinect_Config==kinect_config,:);
        
        for scenario_id = unique(k_table.Scenario_Id,'rows').'
            scen_table = k_table(k_table.Scenario_Id==scenario_id,:);

            fprintf('Participant: %d, Kinect_Config: %d, Scenario_Id: %d\n', participant_id, kinect_config, scenario_id);
            
			joint_types_x = (1:length(joint_types))';
            % Assume one row (one person)
            avg_dx = scen_table{1,first_avg_dx:8:last_idx}.';
			std_dx = scen_table{1,first_avg_dx+1:8:last_idx}.';
			avg_dy = scen_table{1,first_avg_dy:8:last_idx}.';
			std_dy = scen_table{1,first_avg_dy+1:8:last_idx}.';
			avg_dz = scen_table{1,first_avg_dz:8:last_idx}.';
			std_dz = scen_table{1,first_avg_dz+1:8:last_idx}.';
			avg_dd = scen_table{1,first_avg_dd:8:last_idx}.';
			std_dd = scen_table{1,first_avg_dd+1:8:last_idx}.';
            
			figure;
			hold on;
			errorbar(joint_types_x,avg_dx,std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
            errorbar(joint_types_x,avg_dy,std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
			errorbar(joint_types_x,avg_dz,std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
			errorbar(joint_types_x,avg_dd,std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
            box on;
			hold off;

			plot_title = sprintf(title_format, participant_id, kinect_config, scenario_id);
			plot_filename = sprintf(filename_format, participant_id, kinect_config, scenario_id);

			title(plot_title,'Fontsize',15);
			xlabel({'','Joint Types',''},'Fontsize',15);
			ylabel('Distance (cm)','Fontsize',15);
            set(gca,'XLim',[0.5 length(joint_types)+0.5]);
			set(gca,'XTick',1:length(joint_types),'XTickLabel',joint_types);
            ax = gca;
            ax.XTickLabelRotation = -90;
			legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');
            
            set(gcf,'Visible','Off');
			set(gcf,'PaperPositionMode','Manual');
			set(gcf,'PaperUnits','Normalized');
            print('-dsvg','-painters',plot_filename);
        end
    end
end

end
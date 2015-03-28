function [] = plotParticipantJointsAverage(time_average_table)
% 
% Per participant
% 

joints_util;

first_avg_dx = 5;
first_avg_dy = 7;
first_avg_dz = 9;
first_avg_dd = 11;
last_avg_dd = 5+length(joint_types)*7

for participant_id = unique(time_average_table.Study_Id,'rows').'
    s_table = time_average_table(time_average_table.Study_Id==participant_id,:);
    
    for kinect_config = unique(s_table.Kinect_Config,'rows').'
        k_table = s_table(s_table.Kinect_Config==kinect_config,:);
        
        for scenario_id = unique(k_table.Scenario_Id,'rows').'
            scen_table = k_table(k_table.Scenario_Id==scenario_id,:);

			x = joint_types;
			avg_dx = scen_table(:,first_avg_dx:8:last_avg_dd).';
			std_dx = scen_table(:,first_avg_dx+1:8:last_avg_dd).';
			avg_dy = scen_table(:,first_avg_dy:8:last_avg_dd).';
			std_dy = scen_table(:,first_avg_dy+1:8:last_avg_dd).';
			avg_dz = scen_table(:,first_avg_dz:8:last_avg_dd).';
			std_dz = scen_table(:,first_avg_dz+1:8:last_avg_dd).';
			avg_dd = scen_table(:,first_avg_dd:8:last_avg_dd).';
			std_dd = scen_table(:,first_avg_dd+1:8:last_avg_dd).';

			figure;
			hold on;
			errorbar(x,avg_dx,std_dx,'-r','LineStyle','none','Marker','x');
			errorbar(x,avg_dy,std_dy,'-g','LineStyle','none','Marker','x');
			errorbar(x,avg_dz,std_dz,'-b','LineStyle','none','Marker','x');
			errorbar(x,avg_dd,std_dd,'-k','LineStyle','none','Marker','o');
			set(gca,'XLim',[0.5 length(joint_types)+0.5])
			set(gca,'XTick',1:length(joint_types),'XTickLabel',joint_types);
			rotateticklabel(gca, -90);
			hold off;

			plot_title = sprintf('Participant %d (Kinect_Config: %d, Scenario_id: %d) - Joints over time', participant_id, kinect_config, scenario_id);
			plot_filename = sprintf('Plots/Participant_%d_KinectConfig_%d_ScenarioId_%d_Joints_over_time', participant_id, kinect_config, scenario_id);

			title(plot_title);
			x_axis_label = xlabel('Joint Types');
			set(x_axis_label,'Position',get(x_axis_label,'Position')-[0,4,0]);
			ylabel('Distance(cm)');
			legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northeastoutside');

			set(gcf, 'PaperPositionMode', 'manual');
			set(gcf, 'PaperUnits', 'normalized');
			set(gcf, 'PaperPosition', [0 0 1 0.6])
			print('-dsvg', '-painters', plot_filename);

		end
	end
end

end
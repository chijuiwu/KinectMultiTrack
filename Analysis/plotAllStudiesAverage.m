function [] = plotAllStudiesAverage(study_average_table)
% 
% Per participant
% 

joints_util;

avg_dx_idx = 4;
avg_dy_idx = 6;
avg_dz_idx = 8;
avg_dd_idx = 10;

scenario_merged_ids = study_average_table{:,'Kinect_Config'} + study_average_table{:,'Scenario_Id'};

x = all_scenarios_table(:,scenarios_c_scenario);
time_dependent_avg_dd = all_scenarios_table(:,scenarios_c_dd_avg);
time_dependent_sd_dd = all_scenarios_table(:,scenarios_c_dd_sd);
figure;
hold on;
errorbar(x,time_dependent_avg_dd,time_dependent_sd_dd,'-xr');
set(gca,'XLim',[0.5 scenario_count+0.5])
set(gca,'XTick',1:1,'XTickLabel',scenario_types);
set(gca, 'XTick',get(gca,'XTick'),'fontsize',10);
rotateticklabel(gca, -90);
hold off;

title(plot_title);
x_axis_label = xlabel('Scenarios');
set(x_axis_label,'Position',get(x_axis_label,'Position')-[0,1.5,0]);
ylabel('Distance(cm)');
legend('Time indepedent Avg. \Delta x','Location','northeastoutside');

set(gcf, 'PaperPositionMode', 'manual');
set(gcf, 'PaperUnits', 'normalized');
set(gcf, 'PaperPosition', [0 0 1 0.7])
print('-dsvg', '-painters', plot_filename);


for kinect_config = unique(study_average_table.Kinect_Config,'rows').'
    k_table = study_average_table(study_average_table.Kinect_Config==kinect_config,:);
    
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
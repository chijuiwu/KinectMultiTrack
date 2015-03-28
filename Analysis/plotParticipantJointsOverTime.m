function [] = plotParticipantJointsOverTime(difference_table)
% 
% Per participant
% 

joints_util;

first_dx 

for participant_id = unique(difference_table.Study_Id,'rows').'
    s_table = difference_table(difference_table.Study_Id==participant_id,:);
    
    for kinect_config = unique(s_table.Kinect_Config,'rows').'
        k_table = s_table(s_table.Kinect_Config==kinect_config,:);
        
        for scenario_id = unique(k_table.Scenario_Id,'rows').'
            scen_table = k_table(k_table.Scenario_Id==scenario_id,:);

			plot_title = sprintf('Participant %d (Kinect_Config: %d, Scenario_id: %d) - Joints over time', participant_id, kinect_config, scenario_id);
			plot_filename = sprintf('Plots/Participant_%d_KinectConfig_%d_ScenarioId_%d_Joints_over_time', participant_id, kinect_config, scenario_id);

			x = all_joints_table(:,all_c_jointtype);
			
			dx_avg = all_joints_table(:,all_c_dx_avg);
			dx_sd = all_joints_table(:,all_c_dx_sd);
			dy_avg = all_joints_table(:,all_c_dy_avg);
			dy_sd = all_joints_table(:,all_c_dy_sd);
			dz_avg = all_joints_table(:,all_c_dz_avg);
			dz_sd = all_joints_table(:,all_c_dz_sd);
			dd_avg = all_joints_table(:,all_c_dd_avg);
			dd_sd = all_joints_table(:,all_c_dd_sd);

figure;
hold on;
errorbar(x,dx_avg,dx_sd,'-r','LineStyle','none','Marker','x');
errorbar(x,dy_avg,dy_sd,'-g','LineStyle','none','Marker','x');
errorbar(x,dz_avg,dz_sd,'-b','LineStyle','none','Marker','x');
errorbar(x,dd_avg,dd_sd,'-k','LineStyle','none','Marker','o');
set(gca,'XLim',[0.5 joint_count+0.5])
set(gca,'XTick',1:joint_count,'XTickLabel',joint_types);
rotateticklabel(gca, -90);
hold off;


% xd = scen_table{:, 'Tracker_Time'};
% yd = joint_types;
% [x,y] = meshgrid(xd,yd)


x = difference_table(:, 'Tracker_Time');

dx_avg = all_joints_table(:,all_c_dx_avg);
dx_sd = all_joints_table(:,all_c_dx_sd);
dy_avg = all_joints_table(:,all_c_dy_avg);
dy_sd = all_joints_table(:,all_c_dy_sd);
dz_avg = all_joints_table(:,all_c_dz_avg);
dz_sd = all_joints_table(:,all_c_dz_sd);
dd_avg = all_joints_table(:,all_c_dd_avg);
dd_sd = all_joints_table(:,all_c_dd_sd);

figure;
hold on;
errorbar(x,dx_avg,dx_sd,'-r','LineStyle','none','Marker','x');
errorbar(x,dy_avg,dy_sd,'-g','LineStyle','none','Marker','x');
errorbar(x,dz_avg,dz_sd,'-b','LineStyle','none','Marker','x');
errorbar(x,dd_avg,dd_sd,'-k','LineStyle','none','Marker','o');
set(gca,'XLim',[0.5 joint_count+0.5])
set(gca,'XTick',1:joint_count,'XTickLabel',joint_types);
rotateticklabel(gca, -90);
hold off;

title(plot_title);
x_axis_label = xlabel('Joints');
set(x_axis_label,'Position',get(x_axis_label,'Position')-[0,4,0]);
ylabel('Distance(cm)');
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northeastoutside');

set(gcf, 'PaperPositionMode', 'manual');
set(gcf, 'PaperUnits', 'normalized');
set(gcf, 'PaperPosition', [0 0 1 0.6])
print('-dsvg', '-painters', plot_filename);

end
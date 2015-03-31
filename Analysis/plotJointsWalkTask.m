function [] = plotJointsWalkTask(joints_average_study_table, joints_average_scenario_table)
% 
% Per participant joints average
% showing dx, dy, dz, dd for all joints
% 

joints_util;
plot_colors;
% 
% title_format = 'Joints Averages for the Walk Task, with %s Kinects';
% dir = 'Plots/Overall/';
% filename_format = strcat(dir,'Joints_Walk_Task_KinectConfig_%s');
% 
% first_avg_dx = 4;
% first_avg_dy = 6;
% first_avg_dz = 8;
% first_avg_dd = 10;
% last_idx = 3+length(joint_types)*8;
% 
% kinect_config_types = {
%   'Parallel', '45-Degrees-apart','90-Degrees-apart'
% };
% 
% for k = unique(joints_average_study_table.Kinect_Config,'rows').'
%     k_table = joints_average_study_table(joints_average_study_table.Kinect_Config==k,:);
% 
%     for scen_id = unique(k_table.Scenario_Id,'rows').'
%         
%         if (scen_id ~=3)
%             continue;
%         end
%         
%         scen_table = k_table(k_table.Scenario_Id==scen_id,:);
% 
%         fprintf('Plotting Joints Stationary Task - Kinect_Config: %d, Scenario_Id: %d\n', k, scen_id);
% 
%         joint_types_x = (1:length(joint_types))';
%         % Assume one row (one person)
%         avg_dx = scen_table{1,first_avg_dx:8:last_idx}.';
%         std_dx = scen_table{1,first_avg_dx+1:8:last_idx}.';
%         avg_dy = scen_table{1,first_avg_dy:8:last_idx}.';
%         std_dy = scen_table{1,first_avg_dy+1:8:last_idx}.';
%         avg_dz = scen_table{1,first_avg_dz:8:last_idx}.';
%         std_dz = scen_table{1,first_avg_dz+1:8:last_idx}.';
%         avg_dd = scen_table{1,first_avg_dd:8:last_idx}.';
%         std_dd = scen_table{1,first_avg_dd+1:8:last_idx}.';
% 
%         figure;
%         hold on;
%         errorbar(joint_types_x,avg_dx,std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
%         errorbar(joint_types_x,avg_dy,std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
%         errorbar(joint_types_x,avg_dz,std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
%         errorbar(joint_types_x,avg_dd,std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
%         box on;
%         hold off;
%         
%         plot_title = sprintf(title_format, kinect_config_types{1,k+1});
%         plot_filename = sprintf(filename_format, kinect_config_types{1,k+1});
% 
%         title(plot_title,'Fontsize',15);
%         xlabel({'','Joint Types'},'Fontsize',15);
%         ylabel({'Distance (cm)',''},'Fontsize',15);
%         set(gca,'XLim',[0.5 length(joint_types)+0.5]);
%         set(gca,'XTick',1:length(joint_types),'XTickLabel',joint_types,'Fontsize',12);
%         ax = gca;
%         ax.XTickLabelRotation = -90;
%         legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');
% 
%         set(gcf,'Visible','Off');
%         set(gcf,'PaperPositionMode','Manual');
%         set(gcf,'PaperUnits','Normalized');
%         print('-dsvg','-painters',plot_filename);
%     end
% end

% average
scen_id = 3;

joint_types_x = (1:length(joint_types))';

first_avg_dx = 3;
first_avg_dy = 5;
first_avg_dz = 7;
first_avg_dd = 9;
last_idx = 2+length(joint_types)*8;

% Assume one row (one person)
avg_dx = joints_average_scenario_table{scen_id,first_avg_dx:8:last_idx}.';
std_dx = joints_average_scenario_table{scen_id,first_avg_dx+1:8:last_idx}.';
avg_dy = joints_average_scenario_table{scen_id,first_avg_dy:8:last_idx}.';
std_dy = joints_average_scenario_table{scen_id,first_avg_dy+1:8:last_idx}.';
avg_dz = joints_average_scenario_table{scen_id,first_avg_dz:8:last_idx}.';
std_dz = joints_average_scenario_table{scen_id,first_avg_dz+1:8:last_idx}.';
avg_dd = joints_average_scenario_table{scen_id,first_avg_dd:8:last_idx}.';
std_dd = joints_average_scenario_table{scen_id,first_avg_dd+1:8:last_idx}.';

figure;
hold on;
errorbar(joint_types_x,avg_dx,std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
errorbar(joint_types_x,avg_dy,std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
errorbar(joint_types_x,avg_dz,std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
errorbar(joint_types_x,avg_dd,std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
box on;
hold off;

title_format = 'Joints Averages for the Walk Task, with All Kinect Configurations';
dir = 'Plots/Overall/';
filename_format = strcat(dir,'Joints_Walk_Task_KinectConfig_All');
plot_title = sprintf(title_format);
plot_filename = sprintf(filename_format);

title(plot_title,'Fontsize',15);
xlabel({'','Joint Types'},'Fontsize',15);
ylabel({'Distance (cm)',''},'Fontsize',15);
set(gca,'XLim',[0.5 length(joint_types)+0.5]);
set(gca,'XTick',1:length(joint_types),'XTickLabel',joint_types,'Fontsize',12);
ax = gca;
ax.XTickLabelRotation = -90;
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');

set(gcf,'Visible','Off');
set(gcf,'PaperPositionMode','Manual');
set(gcf,'PaperUnits','Normalized');
print('-dsvg','-painters',plot_filename);

end
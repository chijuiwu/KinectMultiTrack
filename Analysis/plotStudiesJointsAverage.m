function [] = plotStudiesJointsAverage(study_joints_table)
% 
% Per participant
% 

joints_util;
plot_colors;

dir = 'Plots/Studies_Average/';

% 
% Overall Average
% 
main_plot_title = 'Studies Overall Average';
main_filename = strcat(dir,'Studies_Overall_Average');

avg_dx_idx = 4;
avg_dy_idx = 6;
avg_dz_idx = 8;
avg_dd_idx = 10;

scenario_merged_types = {
  'Kinect_{0}Stationary', 'Kinect_{0}Steps', 'Kinect_{0}Move', ...
  'Kinect_{45}Stationary', 'Kinect_{45}Steps', 'Kinect_{45}Move', ...
  'Kinect_{90}Stationary', 'Kinect_{90}Steps', 'Kinect_{90}Move', ...
  'Kinect_{90}Obstacle'
};

scenario_merged_types_x = 1:length(scenario_merged_types);
studies_avg_dx = study_joints_table{:,avg_dx_idx};
studies_std_dx = study_joints_table{:,avg_dx_idx+1};
studies_avg_dy = study_joints_table{:,avg_dy_idx};
studies_std_dy = study_joints_table{:,avg_dy_idx+1};
studies_avg_dz = study_joints_table{:,avg_dz_idx};
studies_std_dz = study_joints_table{:,avg_dz_idx+1};
studies_avg_dd = study_joints_table{:,avg_dd_idx};
studies_std_dd = study_joints_table{:,avg_dd_idx+1};
joint_types_z = 1:length(joint_types);

figure;
hold on;
errorbar(scenario_merged_types_x,studies_avg_dx,studies_std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
errorbar(scenario_merged_types_x,studies_avg_dy,studies_std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
errorbar(scenario_merged_types_x,studies_avg_dz,studies_std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
errorbar(scenario_merged_types_x,studies_avg_dd,studies_std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
box on;
hold off;

title(main_plot_title,'Fontsize',15);
xlabel({'','','','','','Scenarios',''},'Fontsize',15);
ylabel('Distance (cm)','Fontsize',15);
set(gca,'XLim',[0.5 length(scenario_merged_types)+0.5]);
set(gca,'XTick',1:length(scenario_merged_types),'XTickLabel',scenario_merged_types);
rotateticklabel(gca, -90);
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');

set(gcf,'Visible','Off');
set(gcf,'PaperPositionMode','Manual');
set(gcf,'PaperUnits','Normalized');
print('-dsvg','-painters',main_filename);

% Kinect Configurations
kinect_config_title = 'Studies Average - [Kinect Configurations]';
kinect_config_filename = strcat(dir,'Studies_Average_KinectConfig');

kinect_config_merged_types = {
  '0', '45', '90'
};

rows = length(unique(study_joints_table.Kinect_Config));

kinect_config_x = 1:length(kinect_config_merged_types);
kinect_config_avg_dx = zeros(rows,1);
kinect_config_std_dx = zeros(rows,1);
kinect_config_avg_dy = zeros(rows,1);
kinect_config_std_dy = zeros(rows,1);
kinect_config_avg_dz = zeros(rows,1);
kinect_config_std_dz = zeros(rows,1);
kinect_config_avg_dd = zeros(rows,1);
kinect_config_std_dd = zeros(rows,1);

row_counter = 1;
for kinect_config = unique(study_joints_table.Kinect_Config,'rows').'
    k_table = study_joints_table(study_joints_table.Kinect_Config==kinect_config&study_joints_table.Scenario_Id~=6,:);
    kinect_config_avg_dx(row_counter,1) = mean(k_table{:,'Joints_avg_dx'});
    kinect_config_std_dx(row_counter,1) = mean(k_table{:,'Joints_avg_dx'});
    kinect_config_avg_dy(row_counter,1) = mean(k_table{:,'Joints_avg_dy'});
    kinect_config_std_dy(row_counter,1) = mean(k_table{:,'Joints_avg_dy'});
    kinect_config_avg_dz(row_counter,1) = mean(k_table{:,'Joints_avg_dz'});
    kinect_config_std_dz(row_counter,1) = mean(k_table{:,'Joints_avg_dz'});
    kinect_config_avg_dd(row_counter,1) = mean(k_table{:,'Joints_avg_dd'});
    kinect_config_std_dd(row_counter,1) = mean(k_table{:,'Joints_avg_dd'});
    row_counter = row_counter+1;
    
end

figure;
hold on;
errorbar(kinect_config_x,kinect_config_avg_dx,kinect_config_std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
errorbar(kinect_config_x,kinect_config_avg_dy,kinect_config_std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
errorbar(kinect_config_x,kinect_config_avg_dz,kinect_config_std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
errorbar(kinect_config_x,kinect_config_avg_dd,kinect_config_std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
box on;
hold off;

title(kinect_config_title,'Fontsize',15);
xlabel({'','Degrees',''},'Fontsize',15);
ylabel('Distance (cm)','Fontsize',15);
set(gca,'XLim',[0.5 length(kinect_config_merged_types)+0.5]);
set(gca,'XTick',1:length(kinect_config_merged_types),'XTickLabel',kinect_config_merged_types);
rotateticklabel(gca, -90);
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');

set(gcf,'Visible','Off');
set(gcf,'PaperPositionMode','Manual');
set(gcf,'PaperUnits','Normalized');
print('-dsvg','-painters',kinect_config_filename);

% 
% Scenarios
% 
scenario_title = 'Studies Average - [Scenarios]';
scenario_filename = strcat(dir,'Studies_Average_Scenarios');

scenario_merged_types = {
  'Stationary', 'Steps', 'Walk'
};

rows = length(unique(study_joints_table.Scenario_Id))-1;

scenario_x = 1:length(scenario_merged_types);
scenario_avg_dx = zeros(rows,1);
scenario_std_dx = zeros(rows,1);
scenario_avg_dy = zeros(rows,1);
scenario_std_dy = zeros(rows,1);
scenario_avg_dz = zeros(rows,1);
scenario_std_dz = zeros(rows,1);
scenario_avg_dd = zeros(rows,1);
scenario_std_dd = zeros(rows,1);

row_counter = 1;
for scen_id = unique(study_joints_table.Scenario_Id,'rows').'
    if (scen_id==6)
        continue;
    end
    
    scen_table = study_joints_table(study_joints_table.Scenario_Id==scen_id,:);
    scenario_avg_dx(row_counter,1) = mean(scen_table{:,'Joints_avg_dx'});
    scenario_std_dx(row_counter,1) = mean(scen_table{:,'Joints_avg_dx'});
    scenario_avg_dy(row_counter,1) = mean(scen_table{:,'Joints_avg_dy'});
    scenario_std_dy(row_counter,1) = mean(scen_table{:,'Joints_avg_dy'});
    scenario_avg_dz(row_counter,1) = mean(scen_table{:,'Joints_avg_dz'});
    scenario_std_dz(row_counter,1) = mean(scen_table{:,'Joints_avg_dz'});
    scenario_avg_dd(row_counter,1) = mean(scen_table{:,'Joints_avg_dd'});
    scenario_std_dd(row_counter,1) = mean(scen_table{:,'Joints_avg_dd'});
    row_counter = row_counter+1;
    
end

figure;
hold on;
errorbar(scenario_x,scenario_avg_dx,scenario_std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
errorbar(scenario_x,scenario_avg_dy,scenario_std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
errorbar(scenario_x,scenario_avg_dz,scenario_std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
errorbar(scenario_x,scenario_avg_dd,scenario_std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
box on;
hold off;

title(scenario_title,'Fontsize',15);
xlabel({'','','','Scenario Types',''},'Fontsize',15);
ylabel('Distance (cm)','Fontsize',15);
set(gca,'XLim',[0.5 length(scenario_merged_types)+0.5]);
set(gca,'XTick',1:length(scenario_merged_types),'XTickLabel',scenario_merged_types);
rotateticklabel(gca, -90);
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');

set(gcf,'Visible','Off');
set(gcf,'PaperPositionMode','Manual');
set(gcf,'PaperUnits','Normalized');
print('-dsvg','-painters',scenario_filename);

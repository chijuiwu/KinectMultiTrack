function [] = plotCoordinatesKinect90(coordinates_average_study_table, coordinates_average_kinect_config_table)

joints_util;
plot_colors;

kinect_config_id = 3;

kinect_scenario_types = {
  'Stationary', 'Steps', 'Walk', 'Average'
};

rows = coordinates_average_study_table.Kinect_Config==kinect_config_id-1 & ...
    (coordinates_average_study_table.Scenario_Id==1 | ...
    coordinates_average_study_table.Scenario_Id==2 | ...
    coordinates_average_study_table.Scenario_Id==3);
k_table = coordinates_average_study_table(rows,:);
rows = size(k_table,1)+1;

kinect_scenario_x = 1:length(kinect_scenario_types);
kinect_scenario_avg_dx = zeros(rows,1);
kinect_scenario_std_dx = zeros(rows,1);
kinect_scenario_avg_dy = zeros(rows,1);
kinect_scenario_std_dy = zeros(rows,1);
kinect_scenario_avg_dz = zeros(rows,1);
kinect_scenario_std_dz = zeros(rows,1);
kinect_scenario_avg_dd = zeros(rows,1);
kinect_scenario_std_dd = zeros(rows,1);

row_counter = 1;
for scne_id = unique(k_table.Scenario_Id,'rows').'
    scen_table = k_table(k_table.Scenario_Id==scne_id,:);
    
    kinect_scenario_avg_dx(row_counter,1) = scen_table{1,'Joints_avg_dx'};
    kinect_scenario_std_dx(row_counter,1) = scen_table{1,'Joints_std_dx'};
    kinect_scenario_avg_dy(row_counter,1) = scen_table{1,'Joints_avg_dy'};
    kinect_scenario_std_dy(row_counter,1) = scen_table{1,'Joints_std_dy'};
    kinect_scenario_avg_dz(row_counter,1) = scen_table{1,'Joints_avg_dz'};
    kinect_scenario_std_dz(row_counter,1) = scen_table{1,'Joints_std_dz'};
    kinect_scenario_avg_dd(row_counter,1) = scen_table{1,'Joints_avg_dd'};
    kinect_scenario_std_dd(row_counter,1) = scen_table{1,'Joints_std_dd'};
    
    row_counter = row_counter+1;
end

kinect_scenario_avg_dx(row_counter,1) = coordinates_average_kinect_config_table{kinect_config_id,'Joints_avg_dx'};
kinect_scenario_std_dx(row_counter,1) = coordinates_average_kinect_config_table{kinect_config_id,'Joints_std_dx'};
kinect_scenario_avg_dy(row_counter,1) = coordinates_average_kinect_config_table{kinect_config_id,'Joints_avg_dy'};
kinect_scenario_std_dy(row_counter,1) = coordinates_average_kinect_config_table{kinect_config_id,'Joints_std_dy'};
kinect_scenario_avg_dz(row_counter,1) = coordinates_average_kinect_config_table{kinect_config_id,'Joints_avg_dz'};
kinect_scenario_std_dz(row_counter,1) = coordinates_average_kinect_config_table{kinect_config_id,'Joints_std_dz'};
kinect_scenario_avg_dd(row_counter,1) = coordinates_average_kinect_config_table{kinect_config_id,'Joints_avg_dd'};
kinect_scenario_std_dd(row_counter,1) = coordinates_average_kinect_config_table{kinect_config_id,'Joints_std_dd'};

figure;
hold on;
errorbar(kinect_scenario_x,kinect_scenario_avg_dx,kinect_scenario_std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
errorbar(kinect_scenario_x,kinect_scenario_avg_dy,kinect_scenario_std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
errorbar(kinect_scenario_x,kinect_scenario_avg_dz,kinect_scenario_std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
errorbar(kinect_scenario_x,kinect_scenario_avg_dd,kinect_scenario_std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
box on;
hold off;

title_format = 'Coordinates Averages wtih 90%c apart Kinects \n for the Stationary, Steps, and Walk Tasks';
dir = '../../KinectMultiTrackPlots/Overall/';
main_filename = strcat(dir,'Coordinates_Kinect_90');

plot_title = sprintf(title_format,char(176));
title(plot_title);
xlabel({'Scenarios'});
ylabel({'Distance (cm)'});
set(gca,'XLim',[0.5 length(kinect_scenario_types)+0.5]);
set(gca,'XTick',1:length(kinect_scenario_types),'XTickLabel',kinect_scenario_types);
% set(xlh,'Fontsize',20);
ax = gca;
ax.XTickLabelRotation = -90;
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');

set(gcf,'Visible','Off');
savepdf(main_filename);
end

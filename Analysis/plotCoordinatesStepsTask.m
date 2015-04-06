function [] = plotCoordinatesStepsTask(coordinates_average_study_table, coordinates_average_scenario_table)

joints_util;
plot_colors;

scenario_id = 2;

kinect_config_types = {
  'Parallel', '45^{\circ}','90^{\circ}','Average'
};

scen_table = coordinates_average_study_table(coordinates_average_study_table.Scenario_Id==scenario_id,:);
rows = size(scen_table,1)+1;

kinect_config_x = 1:length(kinect_config_types);
kinect_config_avg_dx = zeros(rows,1);
kinect_config_std_dx = zeros(rows,1);
kinect_config_avg_dy = zeros(rows,1);
kinect_config_std_dy = zeros(rows,1);
kinect_config_avg_dz = zeros(rows,1);
kinect_config_std_dz = zeros(rows,1);
kinect_config_avg_dd = zeros(rows,1);
kinect_config_std_dd = zeros(rows,1);

row_counter = 1;
for kinect_config = unique(scen_table.Kinect_Config,'rows').'
    k_table = scen_table(scen_table.Kinect_Config==kinect_config,:);
    
    kinect_config_avg_dx(row_counter,1) = k_table{1,'Joints_avg_dx'};
    kinect_config_std_dx(row_counter,1) = k_table{1,'Joints_std_dx'};
    kinect_config_avg_dy(row_counter,1) = k_table{1,'Joints_avg_dy'};
    kinect_config_std_dy(row_counter,1) = k_table{1,'Joints_std_dy'};
    kinect_config_avg_dz(row_counter,1) = k_table{1,'Joints_avg_dz'};
    kinect_config_std_dz(row_counter,1) = k_table{1,'Joints_std_dz'};
    kinect_config_avg_dd(row_counter,1) = k_table{1,'Joints_avg_dd'};
    kinect_config_std_dd(row_counter,1) = k_table{1,'Joints_std_dd'};
    
    row_counter = row_counter+1;
end

kinect_config_avg_dx(row_counter,1) = coordinates_average_scenario_table{scenario_id,'Joints_avg_dx'};
kinect_config_std_dx(row_counter,1) = coordinates_average_scenario_table{scenario_id,'Joints_std_dx'};
kinect_config_avg_dy(row_counter,1) = coordinates_average_scenario_table{scenario_id,'Joints_avg_dy'};
kinect_config_std_dy(row_counter,1) = coordinates_average_scenario_table{scenario_id,'Joints_std_dy'};
kinect_config_avg_dz(row_counter,1) = coordinates_average_scenario_table{scenario_id,'Joints_avg_dz'};
kinect_config_std_dz(row_counter,1) = coordinates_average_scenario_table{scenario_id,'Joints_std_dz'};
kinect_config_avg_dd(row_counter,1) = coordinates_average_scenario_table{scenario_id,'Joints_avg_dd'};
kinect_config_std_dd(row_counter,1) = coordinates_average_scenario_table{scenario_id,'Joints_std_dd'};

figure;
hold on;
errorbar(kinect_config_x,kinect_config_avg_dx,kinect_config_std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
errorbar(kinect_config_x,kinect_config_avg_dy,kinect_config_std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
errorbar(kinect_config_x,kinect_config_avg_dz,kinect_config_std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
errorbar(kinect_config_x,kinect_config_avg_dd,kinect_config_std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
box on;
hold off;

title_format = 'Coordinates Averages in the Steps Task with \n Parallel, 45%c and 90%c apart Kinects';
dir = '../../KinectMultiTrackPlots/Overall/';
main_filename = strcat(dir,'Coordinates_Task_Steps');

plot_title = sprintf(title_format,char(176),char(176));
title(plot_title,'Fontsize',15);
xlabel({'Kinect Configurations'},'Fontsize',15);
ylabel({'Distance (cm)',''},'Fontsize',15);
set(gca,'XLim',[0.5 length(kinect_config_types)+0.5]);
set(gca,'XTick',1:length(kinect_config_types),'XTickLabel',kinect_config_types,'Fontsize',12);
ax = gca;
ax.XTickLabelRotation = -90;
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');

set(gcf,'Visible','Off');
set(gcf,'PaperPositionMode','Manual');
set(gcf,'PaperUnits','Normalized');
print('-dsvg','-painters',main_filename);


end

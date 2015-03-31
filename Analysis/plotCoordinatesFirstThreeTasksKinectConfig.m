function [] = plotCoordinatesFirstThreeTasksKinectConfig(coordinates_average_kinect_config_table)

joints_util;
plot_colors;

main_title = 'Coordinates Averages for Parallel, 45, and 90 Degrees-apart Kinects';
dir = 'Plots/Stationary_Task/';
main_filename = strcat(dir,'Coordinates_First_Three_Tasks_Kinect_Config');

kinect_config_types = {
  'Stationary', 'Steps','Walk','Average'
};

rows = size(coordinates_average_kinect_config_table,1)+1;

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
for r = 1:size(coordinates_average_kinect_config_table,1);
    scen_table = coordinates_average_kinect_config_table(r,:);
    
    kinect_config_avg_dx(row_counter,1) = scen_table{1,'Joints_avg_dx'};
    kinect_config_std_dx(row_counter,1) = scen_table{1,'Joints_sd_dx'};
    kinect_config_avg_dy(row_counter,1) = scen_table{1,'Joints_avg_dy'};
    kinect_config_std_dy(row_counter,1) = scen_table{1,'Joints_sd_dy'};
    kinect_config_avg_dz(row_counter,1) = scen_table{1,'Joints_avg_dz'};
    kinect_config_std_dz(row_counter,1) = scen_table{1,'Joints_sd_dz'};
    kinect_config_avg_dd(row_counter,1) = scen_table{1,'Joints_avg_dd'};
    kinect_config_std_dd(row_counter,1) = scen_table{1,'Joints_sd_dd'};
    
    row_counter = row_counter+1;
end

kinect_config_avg_dx(row_counter,1) = mean(coordinates_average_kinect_config_table{:,'Joints_avg_dx'});
kinect_config_std_dx(row_counter,1) = std(coordinates_average_kinect_config_table{:,'Joints_avg_dx'});
kinect_config_avg_dy(row_counter,1) = mean(coordinates_average_kinect_config_table{:,'Joints_avg_dy'});
kinect_config_std_dy(row_counter,1) = std(coordinates_average_kinect_config_table{:,'Joints_avg_dy'});
kinect_config_avg_dz(row_counter,1) = mean(coordinates_average_kinect_config_table{:,'Joints_avg_dz'});
kinect_config_std_dz(row_counter,1) = std(coordinates_average_kinect_config_table{:,'Joints_avg_dz'});
kinect_config_avg_dd(row_counter,1) = mean(coordinates_average_kinect_config_table{:,'Joints_avg_dd'});
kinect_config_std_dd(row_counter,1) = std(coordinates_average_kinect_config_table{:,'Joints_avg_dd'});

figure;
hold on;
errorbar(kinect_config_x,kinect_config_avg_dx,kinect_config_std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
errorbar(kinect_config_x,kinect_config_avg_dy,kinect_config_std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
errorbar(kinect_config_x,kinect_config_avg_dz,kinect_config_std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
errorbar(kinect_config_x,kinect_config_avg_dd,kinect_config_std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
box on;
hold off;

title(main_title,'Fontsize',15);
xlabel({'','Scenarios',''},'Fontsize',15);
ylabel('Distance (cm)','Fontsize',15);
set(gca,'XLim',[0.5 length(kinect_config_types)+0.5]);
set(gca,'XTick',1:length(kinect_config_types),'XTickLabel',kinect_config_types);
ax = gca;
ax.XTickLabelRotation = -90;
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');

set(gcf,'Visible','Off');
set(gcf,'PaperPositionMode','Manual');
set(gcf,'PaperUnits','Normalized');
print('-dsvg','-painters',main_filename);


end

function [] = plotCoordinatesAllStudies(coordinates_merged_average_study_table)
% 
% Per participant
% 

joints_util;
plot_colors;

dir = '../../KinectMultiTrackPlots/Overall/';

% 
% Overall Average
% 
main_plot_title = 'Coordinates Averages for all Kinect Configurations and Scenarios';
main_filename = strcat(dir,'Coordinates_All');

all_experiment_types = {
  'Parallel, Stationary', 'Parallel, Steps', 'Parallel, Walk', ...
  '45 Degrees, Stationary', '45 Degrees, Steps', '45 Degrees, Walk', ...
  '45 Degrees, Interaction', ...
  '90 Degrees, Stationary', '90 Degrees, Steps', '90 Degrees, Walk', ...
  '90 Degrees, Obstacle'
};

all_experiment_types_x = 1:length(all_experiment_types);

studies_avg_dx = coordinates_merged_average_study_table{:,'Joints_avg_dx'};
studies_std_dx = coordinates_merged_average_study_table{:,'Joints_std_dx'};
studies_avg_dy = coordinates_merged_average_study_table{:,'Joints_avg_dy'};
studies_std_dy = coordinates_merged_average_study_table{:,'Joints_std_dy'};
studies_avg_dz = coordinates_merged_average_study_table{:,'Joints_avg_dz'};
studies_std_dz = coordinates_merged_average_study_table{:,'Joints_std_dz'};
studies_avg_dd = coordinates_merged_average_study_table{:,'Joints_avg_dd'};
studies_std_dd = coordinates_merged_average_study_table{:,'Joints_std_dd'};

figure;
hold on;
errorbar(all_experiment_types_x,studies_avg_dx,studies_std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
errorbar(all_experiment_types_x,studies_avg_dy,studies_std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
errorbar(all_experiment_types_x,studies_avg_dz,studies_std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
errorbar(all_experiment_types_x,studies_avg_dd,studies_std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
box on;
hold off;

title(main_plot_title,'Fontsize',15);
xlabel({'','Scenarios'},'Fontsize',15);
ylabel({'Distance (cm)',''},'Fontsize',15);
set(gca,'XLim',[0.5 length(all_experiment_types)+0.5]);
set(gca,'XTick',1:length(all_experiment_types),'XTickLabel',all_experiment_types,'Fontsize',12);
ax = gca;
ax.XTickLabelRotation = -90;
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');

set(gcf,'Visible','Off');
set(gcf,'PaperPositionMode','Manual');
set(gcf,'PaperUnits','Normalized');
print('-dsvg','-painters',main_filename);
end
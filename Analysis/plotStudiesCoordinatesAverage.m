function [] = plotStudiesCoordinatesAverage(study_average_table)
% 
% Per participant
% 

joints_util;

plot_title = 'Studies Coordinates Average';
dir = 'Plots/Studies_Average/';
filename = strcat(dir,'Studies_Coordinates_Average');

avg_dx_idx = 4;
avg_dy_idx = 6;
avg_dz_idx = 8;
avg_dd_idx = 10;

red = [.549 .086 .086];
green = [0 .389 .247];
blue = [.118 .259 0.651];
black = [0 0 0];

scenario_merged_types = {
  'Kinect_{0}Stationary', 'Kinect_{0}Walk', 'Kinect_{0}Move', ...
  'Kinect_{45}Stationary', 'Kinect_{45}Walk', 'Kinect_{45}Move', ...
  'Kinect_{90}Stationary', 'Kinect_{90}Walk', 'Kinect_{90}Move', ...
  'Kinect_{90}Obstacle'
};

x = 1:length(scenario_merged_types);
studies_avg_dx = study_average_table{:,avg_dx_idx};
studies_std_dx = study_average_table{:,avg_dx_idx+1};
studies_avg_dy = study_average_table{:,avg_dy_idx};
studies_std_dy = study_average_table{:,avg_dy_idx+1};
studies_avg_dz = study_average_table{:,avg_dz_idx};
studies_std_dz = study_average_table{:,avg_dz_idx+1};
studies_avg_dd = study_average_table{:,avg_dd_idx};
studies_std_dd = study_average_table{:,avg_dd_idx+1};

figure;
hold on;
errorbar(x,studies_avg_dx,studies_std_dx,'MarkerEdgeColor',red,'MarkerFaceColor',red,'Color',red,'LineStyle','none','Marker','o');
errorbar(x,studies_avg_dy,studies_std_dy,'MarkerEdgeColor',green,'MarkerFaceColor',green,'Color',green,'LineStyle','none','Marker','o');
errorbar(x,studies_avg_dz,studies_std_dz,'MarkerEdgeColor',blue,'MarkerFaceColor',blue,'Color',blue,'LineStyle','none','Marker','o');
errorbar(x,studies_avg_dd,studies_std_dd,'MarkerEdgeColor',black,'MarkerFaceColor',black,'Color',black,'LineStyle','none','Marker','x','MarkerSize',10);
box on;
hold off;

title(plot_title,'Fontsize',15);
xlabel({'','','','','','Scenarios',''},'Fontsize',15);
ylabel('Distance (cm)','Fontsize',15);
set(gca,'XLim',[0.5 length(scenario_merged_types)+0.5]);
set(gca,'XTick',1:length(scenario_merged_types),'XTickLabel',scenario_merged_types);
rotateticklabel(gca, -90);
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northwest');

set(gcf,'Visible','Off');
set(gcf,'PaperPositionMode','Manual');
set(gcf,'PaperUnits','Normalized');
print('-dsvg','-painters',filename);

% Kinect Configurations

plot_title = 'Studies Coordinates Average';
dir = 'Plots/Studies_Average/';
filename = strcat(dir,'Studies_Coordinates_Average');

for kinect_config = unique(s_table.Kinect_Config,'rows').'
    k_table = s_table(s_table.Kinect_Config==kinect_config,:);


% Studies

for scenario_id = unique(k_table.Scenario_Id,'rows').'
    scen_table = k_table(k_table.Scenario_Id==scenario_id,:);

end

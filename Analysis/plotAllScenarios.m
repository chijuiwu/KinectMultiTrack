function [] = plotAllScenarios(all_scenarios_table)
consts;

person_id = all_scenarios_table(1,scenarios_c_person);
plot_title = sprintf('Person %d All Scenarios',person_id);
plot_filename = sprintf('Plots/Person_%d_AllScenarios',person_id);

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

end


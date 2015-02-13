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
% set(gcf,'visible','off');
% ylim([0 0.2]);
set(gca,'XTick',1:1,'XTickLabel',scenario_types);
hold off;

title(plot_title);
xlabel('Scenarios');
ylabel('Distance(m)');
legend('Time depedent Avg. \Delta x','Location','northeastoutside');
print('-dpdf', '-painters', plot_filename);

end


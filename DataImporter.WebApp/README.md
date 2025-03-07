# SetaInterview

##Step-by-Step Guide to Build and Run for Developers

Prerequisite settings:
This project was installed with .NET 9, the newest version. Please ensure your local machine has .NET 9 installed.

1. Open in Visual Studio: Use Visual Studio 2022 or later to open the .sln file: "DataImporter.WebApp/DataImporter.WebApp.sln".
2. Restore NuGet packages: Right-click the solution in Solution Explorer and select "Restore NuGet Packages" (includes CsvHelper, etc.).
3. Add the CSV file: Place sampleSheet.csv in the wwwroot/data folder (DataImporter.WebApp/wwwroot).
4. Build the solution: Press Ctrl+Shift+B or use the Build menu.
5. Run the application: Press F5 to launch the web app in your browser.
6. Explore the app: Navigate to the homepage to see the statistics, graph, and table data.

Notes: 
1. Unit test coverage is included
2. Any exceptions will be shown on the UI as an example; in real projects, we'll store them in the logging system
3. Ensure sampleSheet.csv is in wwwroot/data, or the app won’t find it.
4. The app assumes the CSV has Date and Market Price EX1 columns in the correct format.
5. Customize the graph or table further by adjusting the Chart.js or DataTables options in the code.

----------------------------------------------------------

##Explanatory Notes

How to zoom the chart
1. **Using mouse wheel**
   - Scroll up: Zoom in
   - Scroll down: Zoom out

2. **Area zoom**
   - Click and drag to create a selection box
   - Selected area will automatically zoom in
   
How to use the table features
1. **Filtering Data**
   - **Date Range Filter**: 
     - Use the two datetime inputs to filter by date range
     - Left input: Start date
     - Right input: End date
     - You can use either or both inputs
   - **Price Filter**: 
     - Enter a price value to filter market prices
     - Shows all prices that start with the entered value
   - **Clear Filter**: 
	 - Click "Clear filters" to reset all filters
2. **Sorting**
   - Click on column headers to sort
   - First click: Sort ascending (↑)
   - Second click: Sort descending (↓)
   - Third click: Remove sorting
   - You can only sort one column at a time

3. **Pagination**
   - Use the number buttons to navigate between pages
   - "<<" and ">>" buttons for quick navigation to the first and the last page
   - "<" and ">" buttons to go to the previous or the next page
   - Enter a specific page number in "Go to page" box
   - Click "Go" or press Enter to jump to that page

4. **Items per Page**
   - Select how many items to show per page (10, 20, or 50)
   - Changes will reset current filters and sorting
   - Table will return to first page
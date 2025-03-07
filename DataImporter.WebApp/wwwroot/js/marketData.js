function initializeMarketData(marketData) {
    if (marketData && marketData.length > 0) {
        const tableState = new TableState(marketData);
        const tableOperations = new TableOperations(tableState);
        tableOperations.updateTableAndPagination();
    }
}

// Constants
const TABLE_CONFIG = {
    PAGINATION_RANGE: 5,
    DEFAULT_ROWS_PER_PAGE: 10,
    SORT_DIRECTIONS: {
        NONE: 'none',
        ASC: 'asc',
        DESC: 'desc'
    }
};

// State Management
class TableState {
    constructor(marketData) {
        this.marketData = marketData;
        this.currentPage = 1;
        this.rowsPerPage = parseInt(document.getElementById('itemsPerPageSelect').value, 10);
        this.sortDirections = { 0: TABLE_CONFIG.SORT_DIRECTIONS.NONE, 1: TABLE_CONFIG.SORT_DIRECTIONS.NONE };
        this.pagedData = [];
        this.totalPages = 0;
    }

    resetFilters() {
        document.getElementById('startDateFilter').value = '';
        document.getElementById('endDateFilter').value = '';
        document.getElementById('priceFilter').value = '';
    }

    resetSort() {
        this.sortDirections = { 0: TABLE_CONFIG.SORT_DIRECTIONS.NONE, 1: TABLE_CONFIG.SORT_DIRECTIONS.NONE };
        document.querySelectorAll('.header-row th i').forEach(icon => {
            icon.className = 'fas fa-sort';
        });
    }

    resetPagination() {
        this.currentPage = 1;
        document.getElementById('pageNumberInput').value = 1;
    }
}

// Table Operations
class TableOperations {
    constructor(tableState) {
        this.state = tableState;
        this.initializeEventListeners();
        this.initializeChart();
    }

    initializeEventListeners() {
        // Sort listeners
        document.querySelectorAll('.header-row th').forEach((th, index) => {
            th.onclick = () => this.handleSort(index);
        });

        // Filter listeners
        document.getElementById('startDateFilter').addEventListener('input', () => this.handleFilter());
        document.getElementById('endDateFilter').addEventListener('input', () => this.handleFilter());
        document.getElementById('priceFilter').addEventListener('input', () => this.handleFilter());

        // Pagination listeners
        document.getElementById('itemsPerPageSelect').addEventListener('change', () => this.handleRowsPerPageChange());
        document.getElementById('goToPageButton').addEventListener('click', () => this.handleGoToPage());
        document.getElementById('clearFiltersButton').addEventListener('click', () => this.handleClearFilters());
        document.getElementById('pageNumberInput').addEventListener('keypress', (event) => {
            if (event.key === 'Enter') {
                event.preventDefault();
                this.handleGoToPage();
            }
        });
    }

    handleSort(columnIndex) {
        const headers = document.querySelectorAll('.header-row th');
        const currentHeader = headers[columnIndex];
        const icon = currentHeader.querySelector('i');

        // Update sort direction
        switch (this.state.sortDirections[columnIndex]) {
            case TABLE_CONFIG.SORT_DIRECTIONS.NONE:
                this.state.sortDirections[columnIndex] = TABLE_CONFIG.SORT_DIRECTIONS.ASC;
                icon.className = 'fas fa-sort-up';
                break;
            case TABLE_CONFIG.SORT_DIRECTIONS.ASC:
                this.state.sortDirections[columnIndex] = TABLE_CONFIG.SORT_DIRECTIONS.DESC;
                icon.className = 'fas fa-sort-down';
                break;
            case TABLE_CONFIG.SORT_DIRECTIONS.DESC:
                this.state.sortDirections[columnIndex] = TABLE_CONFIG.SORT_DIRECTIONS.NONE;
                icon.className = 'fas fa-sort';
                break;
        }

        // Reset other columns
        headers.forEach((header, index) => {
            if (index !== columnIndex) {
                header.querySelector('i').className = 'fas fa-sort';
                this.state.sortDirections[index] = TABLE_CONFIG.SORT_DIRECTIONS.NONE;
            }
        });

        this.updateTableAndPagination();
    }

    handleFilter() {
        const startDate = document.getElementById('startDateFilter').value;
        const endDate = document.getElementById('endDateFilter').value;
        const dateFilter = {
            startDate: startDate,
            endDate: endDate
        };
        const priceFilter = document.getElementById('priceFilter').value;

        this.state.currentPage = 1;
        this.updateTableAndPagination();
    }

    handleRowsPerPageChange() {
        this.state.rowsPerPage = parseInt(document.getElementById('itemsPerPageSelect').value, 10);
        this.state.resetFilters();
        this.state.resetSort();
        this.state.resetPagination();
        this.updateTableAndPagination();
    }

    handleGoToPage() {
        const pageNumber = parseInt(document.getElementById('pageNumberInput').value, 10);
        if (!isNaN(pageNumber) && pageNumber >= 1 && pageNumber <= this.state.totalPages) {
            this.state.currentPage = pageNumber;
            this.updateTableAndPagination();
        } else {
            alert(`Invalid page number. Please enter a number between 1 and ${this.state.totalPages}`);
            document.getElementById('pageNumberInput').value = this.state.currentPage;
        }
    }

    handleClearFilters() {
        this.state.resetFilters();
        this.state.resetSort();
        this.state.resetPagination();
        this.updateTableAndPagination();
    }

    getFilteredAndSortedData() {
        let filteredData = [...this.state.marketData];
        const dateFilter = {
            startDate: document.getElementById('startDateFilter').value,
            endDate: document.getElementById('endDateFilter').value
        };
        const priceFilter = document.getElementById('priceFilter').value;

        // Apply filters
        if (dateFilter.startDate || dateFilter.endDate || priceFilter) {
            filteredData = this.filterData(filteredData, dateFilter, priceFilter);
        }

        // Apply sort
        filteredData = this.sortData(filteredData);

        return filteredData;
    }

    filterData(data, dateFilter, priceFilter) {
        return data.filter(item => {
            let showRow = true;

            if (dateFilter.startDate || dateFilter.endDate) {
                const itemDate = new Date(item.date);

                if (dateFilter.startDate) {
                    const startDate = new Date(dateFilter.startDate);
                    if (itemDate < startDate) {
                        showRow = false;
                    }
                }

                if (dateFilter.endDate) {
                    const endDate = new Date(dateFilter.endDate);
                    if (itemDate > endDate) {
                        showRow = false;
                    }
                }
            }

            if (priceFilter && priceFilter.trim() !== '') {
                const priceString = item.marketPrice.toString();
                if (!priceString.startsWith(priceFilter)) {
                    showRow = false;
                }
            }

            return showRow;
        });
    }

    sortData(data) {
        for (let columnIndex in this.state.sortDirections) {
            if (this.state.sortDirections[columnIndex] !== TABLE_CONFIG.SORT_DIRECTIONS.NONE) {
                const direction = this.state.sortDirections[columnIndex];
                data.sort((a, b) => {
                    const aValue = columnIndex === '0' ? new Date(a.date) : a.marketPrice;
                    const bValue = columnIndex === '0' ? new Date(b.date) : b.marketPrice;
                    return direction === TABLE_CONFIG.SORT_DIRECTIONS.ASC ?
                        (aValue > bValue ? 1 : -1) :
                        (aValue < bValue ? 1 : -1);
                });
                break;
            }
        }
        return data;
    }

    updateTableAndPagination() {
        const filteredAndSortedData = this.getFilteredAndSortedData();
        this.state.totalPages = Math.ceil(filteredAndSortedData.length / this.state.rowsPerPage);

        // Update paged data
        const startIndex = (this.state.currentPage - 1) * this.state.rowsPerPage;
        const endIndex = startIndex + this.state.rowsPerPage;
        this.state.pagedData = filteredAndSortedData.slice(startIndex, endIndex);

        this.renderTable();
        this.renderPagination();
    }

    renderTable() {
        const tableBody = document.querySelector('#marketDataTable tbody');
        tableBody.innerHTML = '';

        this.state.pagedData.forEach(data => {
            const row = tableBody.insertRow();
            const dateCell = row.insertCell();
            const priceCell = row.insertCell();
            dateCell.textContent = new Date(data.date).toLocaleString();
            priceCell.textContent = data.marketPrice;
        });
    }

    renderPagination() {
        const container = document.getElementById('pagination');
        container.innerHTML = '';

        // Calculate page range
        let { startPage, endPage } = this.calculatePageRange();

        // Render pagination elements
        this.renderPaginationControls(container, startPage, endPage);
    }

    calculatePageRange() {
        let startPage = this.state.currentPage - Math.floor(TABLE_CONFIG.PAGINATION_RANGE / 2);
        let endPage = this.state.currentPage + Math.floor(TABLE_CONFIG.PAGINATION_RANGE / 2);

        if (startPage <= 0) {
            endPage -= (startPage - 1);
            startPage = 1;
        }

        if (endPage > this.state.totalPages) {
            startPage -= (endPage - this.state.totalPages);
            if (startPage < 1) startPage = 1;
            endPage = this.state.totalPages;
        }

        return { startPage, endPage };
    }

    renderPaginationControls(container, startPage, endPage) {
        // First page button
        const firstPageButton = document.createElement('a');
        firstPageButton.href = '#';
        firstPageButton.innerHTML = '<i class="fas fa-angle-double-left"></i>';
        firstPageButton.className = `pagination-button nav-btn ${this.state.currentPage === 1 ? 'disabled' : ''}`;
        firstPageButton.onclick = (e) => {
            e.preventDefault();
            if (this.state.currentPage !== 1) {
                this.state.currentPage = 1;
                this.updateTableAndPagination();
            }
        };
        container.appendChild(firstPageButton);

        // Previous button
        this.addPaginationButton(container, 'prev');

        // First page and dots
        if (startPage > 1) {
            this.addPageButton(container, 1);
            if (startPage > 2) this.addPaginationDots(container);
        }

        // Page numbers
        for (let i = startPage; i <= endPage; i++) {
            this.addPageButton(container, i);
        }

        // Last page and dots
        if (endPage < this.state.totalPages) {
            if (endPage < this.state.totalPages - 1) this.addPaginationDots(container);
            this.addPageButton(container, this.state.totalPages);
        }

        // Next button
        this.addPaginationButton(container, 'next');

        // Last page button
        const lastPageButton = document.createElement('a');
        lastPageButton.href = '#';
        lastPageButton.innerHTML = '<i class="fas fa-angle-double-right"></i>';
        lastPageButton.className = `pagination-button nav-btn ${this.state.currentPage === this.state.totalPages ? 'disabled' : ''}`;
        lastPageButton.onclick = (e) => {
            e.preventDefault();
            if (this.state.currentPage !== this.state.totalPages) {
                this.state.currentPage = this.state.totalPages;
                this.updateTableAndPagination();
            }
        };
        container.appendChild(lastPageButton);
    }

    addPaginationButton(container, type) {
        const button = document.createElement('a');
        button.href = '#';

        if (type === 'prev') {
            button.innerHTML = '<i class="fas fa-chevron-left"></i>';
            button.className = `pagination-button nav-btn ${this.state.currentPage === 1 ? 'disabled' : ''}`;
            button.onclick = (e) => {
                e.preventDefault();
                if (this.state.currentPage > 1) {
                    this.state.currentPage--;
                    this.updateTableAndPagination();
                }
            };
        } else {
            button.innerHTML = '<i class="fas fa-chevron-right"></i>';
            button.className = `pagination-button nav-btn ${this.state.currentPage === this.state.totalPages ? 'disabled' : ''}`;
            button.onclick = (e) => {
                e.preventDefault();
                if (this.state.currentPage < this.state.totalPages) {
                    this.state.currentPage++;
                    this.updateTableAndPagination();
                }
            };
        }

        container.appendChild(button);
    }

    addPageButton(container, pageNumber) {
        const button = document.createElement('a');
        button.href = '#';
        button.textContent = pageNumber;
        button.className = `pagination-button ${this.state.currentPage === pageNumber ? 'active' : ''}`;
        button.onclick = (e) => {
            e.preventDefault();
            this.state.currentPage = pageNumber;
            this.updateTableAndPagination();
        };
        container.appendChild(button);
    }

    addPaginationDots(container) {
        const dots = document.createElement('span');
        dots.textContent = '...';
        dots.className = 'pagination-dots';
        container.appendChild(dots);
    }

    initializeChart() {
        const dates = this.state.marketData.map(data => new Date(data.date));
        const prices = this.state.marketData.map(data => data.marketPrice);
        const ctx = document.getElementById('marketChart').getContext('2d');

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: dates,
                datasets: [{
                    label: 'Market Price',
                    data: prices,
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.1)',
                    borderWidth: 2,
                    pointRadius: 3,
                    pointHoverRadius: 5,
                    fill: false,
                    showLine: false
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'index'
                },
                plugins: {
                    zoom: {
                        pan: {
                            enabled: true,
                            mode: 'xy'
                        },
                        zoom: {
                            wheel: {
                                enabled: true,
                            },
                            pinch: {
                                enabled: true
                            },
                            mode: 'xy',
                            drag: {
                                enabled: true,
                                backgroundColor: 'rgba(75, 192, 192, 0.1)',
                                borderColor: 'rgb(75, 192, 192)',
                                borderWidth: 1,
                                threshold: 5
                            },
                            limits: {
                                y: { min: 'original', max: 'original' },
                                x: { min: 'original', max: 'original' }
                            }
                        },
                        limits: {
                            x: { min: 'original', max: 'original' },
                            y: { min: 'original', max: 'original' }
                        }
                    },
                    legend: {
                        position: 'top',
                        labels: {
                            usePointStyle: true,
                            padding: 15
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(255, 255, 255, 0.9)',
                        titleColor: '#000',
                        bodyColor: '#000',
                        borderColor: 'rgb(75, 192, 192)',
                        borderWidth: 1,
                        padding: 10,
                        displayColors: false,
                        callbacks: {
                            title: (tooltipItems) => {
                                const date = new Date(tooltipItems[0].parsed.x);
                                return date.toLocaleString();
                            },
                            label: (context) => {
                                return `Price: $${context.parsed.y.toFixed(2)}`;
                            }
                        }
                    }
                },
                layout: {
                    padding: {
                        bottom: 20
                    }
                },
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: 'month',
                            tooltipFormat: 'MMM d, yyyy h:mm a',
                            displayFormats: {
                                month: 'MMM yyyy'
                            }
                        },
                        grid: {
                            display: false
                        },
                        title: {
                            display: true,
                            text: 'Date/Time',
                            padding: {
                                top: 10
                            }
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(0, 0, 0, 0.1)'
                        },
                        title: {
                            display: true,
                            text: 'Market Price ($)',
                            padding: {
                                bottom: 10
                            }
                        },
                        ticks: {
                            callback: (value) => `$${value}`
                        }
                    }
                }
            }
        });
    }
}
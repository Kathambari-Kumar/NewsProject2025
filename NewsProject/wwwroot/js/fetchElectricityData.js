async function fetchElectricityData() {
    const date = new Date().toISOString().split('T')[0]; // Get current date in "yyyy-MM-dd" format
    try {
        // Make the API call with the correct URL
        const response = await fetch(`/API/API/HourlyElectricity?date=${date}`);

        // If the response is not successful, handle it
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        // Parse the response JSON
        const data = await response.json();


        // Check if the zones data exists and is valid
        if (data && data.zones) {
            const tableBody = document.querySelector('#electricity-table tbody');
            tableBody.innerHTML = ''; // Clear existing rows

            // Loop through regions and populate table
            Object.keys(data.zones).forEach(region => {
                data.zones[region].forEach(record => {
                    // Ensure price_eur and priceek are valid numbers before calling toFixed
                    const priceEUR = (typeof record.price_eur === 'number' && !isNaN(record.price_eur)) ? record.price_eur.toFixed(2) : 'N/A';
                    const priceSEK = (typeof record.price_sek === 'number' && !isNaN(record.price_sek)) ? record.price_sek.toFixed(2) : 'N/A';



                    const row = document.createElement('tr');
                    row.innerHTML = `
                                                <td>${region}</td>
                                                <td>${record.hour}</td>
                                                <td>${priceEUR}</td>
                                                <td>${priceSEK}</td>
                                                <td>
                                                    ${record.kMeans === 0 ? "Very Low" :
                            record.kMeans === 1 ? "Moderate" :
                                record.kMeans === 2 ? "High" :
                                    record.kMeans === 3 ? "Peak" : "Unknown"
                        }
                                                </td>
                                            `;
                    tableBody.appendChild(row);
                });
            });
        } else {
            console.error("Error: Zones data is missing or malformed.");
            const tableBody = document.querySelector('#electricity-table tbody');
            tableBody.innerHTML = `<tr><td colspan="5">Error loading data. Please try again later.</td></tr>`;
        }
    } catch (error) {
        console.error('Error fetching data:', error);
        const tableBody = document.querySelector('#electricity-table tbody');
        tableBody.innerHTML = `<tr><td colspan="5">Error loading data. Please try again later.</td></tr>`;
    }
}

// Fetch data when the page loads
fetchElectricityData();

// Refresh data every hour
setInterval(fetchElectricityData, 60 * 60 * 1000);
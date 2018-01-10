function runSql(query) {

	var customers = JSON.parse(exec(query));

	return customers;
}
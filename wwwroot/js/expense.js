<script>
    async function editExpense(expenseId) {
    try {
        // ✅ Fetch data from API
        let response = await fetch(`/api/Employee/getexpense/${expenseId}`);
    if (!response.ok) throw new Error("Expense not found");

    let data = await response.json();

    // ✅ Populate your edit form fields
    document.getElementById("expenseId").value = data.id;
    document.getElementById("typeOfExpense").value = data.typeOfExpense;
    document.getElementById("quantity").value = data.quantity;
    document.getElementById("fellowMembers").value = data.fellowMembers;
    document.getElementById("claimAmount").value = data.claimAmount;
    document.getElementById("siteName").value = data.siteName;
    document.getElementById("projectCode").value = data.projectCode;
    document.getElementById("status").value = data.status;

    // Show edit modal
    document.getElementById("editModal").style.display = "block";
    } catch (err) {
        console.error(err);
    alert("Error loading expense!");
    }
}

    async function saveExpense() {
        let expenseId = document.getElementById("expenseId").value;

    let updatedExpense = {
        id: parseInt(expenseId),
    typeOfExpense: document.getElementById("typeOfExpense").value,
    quantity: parseInt(document.getElementById("quantity").value),
    fellowMembers: document.getElementById("fellowMembers").value,
    claimAmount: parseFloat(document.getElementById("claimAmount").value),
    siteName: document.getElementById("siteName").value,
    projectCode: document.getElementById("projectCode").value,
    status: document.getElementById("status").value
    };

    let response = await fetch(`/api/Employee/updateexpense/${expenseId}`, {
        method: "PUT",
    headers: {"Content-Type": "application/json" },
    body: JSON.stringify(updatedExpense)
    });

    if (response.ok) {
        alert("Expense updated successfully!");
    location.reload(); // Refresh list after save
    } else {
        alert("Error saving expense!");
    }
}
</script>

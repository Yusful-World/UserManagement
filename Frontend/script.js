const API_BASE_URL = 'http://localhost:5285/api/v1/User';
        
        // Application State
        let users = [];
        let filteredUsers = [];
        let currentPage = 1;
        const usersPerPage = 10;
        let isEditMode = false;
        let editingUserId = null;
        let currentImageData = null;
        let isBulkDeleteMode = false;
        let selectedUsers = new Set();
        let searchTimeout;

        // Initialize the app
        document.addEventListener('DOMContentLoaded', function() {
            setupEventListeners();
            loadUsers();
        });

        function setupEventListeners() {
            // Search functionality
            document.getElementById('searchInput').addEventListener('input', handleSearch);
            
            // Image upload drag and drop
            const imageUpload = document.querySelector('.image-upload');
            imageUpload.addEventListener('dragover', handleDragOver);
            imageUpload.addEventListener('drop', handleDrop);
            imageUpload.addEventListener('dragleave', handleDragLeave);

            // Form submission
            document.getElementById('userForm').addEventListener('submit', handleFormSubmit);

            // Close modal on outside click
            document.getElementById('userModal').addEventListener('click', function(e) {
                if (e.target === this) {
                    closeModal();
                }
            });

            // Keyboard shortcuts
            document.addEventListener('keydown', function(e) {
                if (e.key === 'Escape') {
                    closeModal();
                }
                if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
                    e.preventDefault();
                    openCreateModal();
                }
            });
        }

        // API Functions
        async function apiRequest(url, options = {}) {
            console.log('API Request URL:', url);
            console.log('API Request Method:', options.method || 'GET');
            
            try {
                const response = await fetch(url, {
                    headers: {
                        'Content-Type': 'application/json',
                        ...options.headers
                    },
                    ...options
                });

                console.log('Response Status:', response.status, response.statusText);

                if (!response.ok) {
                    const errorText = await response.text();
                    console.error('Error Response Text:', errorText);
                    
                    let errorData = {};
                    try {
                        errorData = JSON.parse(errorText);
                        console.error('Parsed Error Response:', errorData);
                    } catch (parseError) {
                        console.error('Could not parse error response as JSON:', parseError);
                        errorData = { message: errorText };
                    }
                    
                    throw new Error(errorData.message || errorData.error || errorText || `HTTP error! status: ${response.status}`);
                }

                const responseText = await response.text();
                console.log('Raw Response Text:', responseText);
                
                let responseData = {};
                try {
                    responseData = JSON.parse(responseText);
                    console.log('Parsed API Response Data:', responseData);
                } catch (parseError) {
                    console.error('Could not parse response as JSON:', parseError);
                    responseData = { message: responseText };
                }
                
                return responseData;
            } catch (error) {
                console.error('API Request Failed:', error);
                showNotification(`API Error: ${error.message}`, 'error');
                throw error;
            }
        }

        function handleImageUpload(event) {
            const file = event.target.files[0];
            if (!file) return;

            // Validate file size (5MB max)
            if (file.size > 5 * 1024 * 1024) {
                showNotification('Image size must be less than 5MB!', 'error');
                return;
            }

            // Validate file type
            if (!file.type.startsWith('image/')) {
                showNotification('Please select a valid image file!', 'error');
                return;
            }

            const reader = new FileReader();
            reader.onload = function(e) {
                currentImageData = e.target.result;
                document.getElementById('imagePreview').src = currentImageData;
                document.getElementById('imagePreview').style.display = 'block';
                document.getElementById('imageUploadText').style.display = 'none';
            };
            reader.readAsDataURL(file);
        }

        function handleDragOver(e) {
            e.preventDefault();
            e.currentTarget.classList.add('dragover');
        }

        function handleDrop(e) {
            e.preventDefault();
            e.currentTarget.classList.remove('dragover');
            
            const files = e.dataTransfer.files;
            if (files.length > 0) {
                document.getElementById('pictureInput').files = files;
                handleImageUpload({ target: { files: files } });
            }
        }

        function handleDragLeave(e) {
            e.currentTarget.classList.remove('dragover');
        }

        function showNotification(message, type) {
            const notification = document.getElementById('notification');
            notification.textContent = message;
            notification.className = `notification ${type}`;
            notification.classList.add('show');

            setTimeout(() => {
                notification.classList.remove('show');
            }, 4000);
        }

        function togglePassword() {
            const passwordInput = document.getElementById('password');
            const toggleBtn = document.querySelector('.password-toggle');
            
            if (passwordInput.type === 'password') {
                passwordInput.type = 'text';
                toggleBtn.textContent = 'hide password';
            } else {
                passwordInput.type = 'password';
                toggleBtn.textContent = 'show password';
            }
        }

        // Bulk Delete Functions
        function toggleBulkDelete() {
            isBulkDeleteMode = !isBulkDeleteMode;
            selectedUsers.clear();
            
            const bulkDeleteBtn = document.getElementById('bulkDeleteBtn');
            if (isBulkDeleteMode) {
                bulkDeleteBtn.textContent = 'Cancel Selection';
                bulkDeleteBtn.classList.remove('btn-secondary');
                bulkDeleteBtn.classList.add('btn-danger');
            } else {
                bulkDeleteBtn.textContent = 'Delete Selected Users';
                bulkDeleteBtn.classList.remove('btn-danger');
                bulkDeleteBtn.classList.add('btn-secondary');
                document.getElementById('floatingDeleteBtn').classList.remove('show');
            }
            
            renderUsers();
        }

        function cancelBulkDelete() {
            toggleBulkDelete();
        }

        function toggleSelectAll(checkbox) {
            const userCheckboxes = document.querySelectorAll('.user-checkbox');
            
            if (checkbox.checked) {
                userCheckboxes.forEach(cb => {
                    cb.checked = true;
                    selectedUsers.add(cb.value);
                });
            } else {
                userCheckboxes.forEach(cb => {
                    cb.checked = false;
                    selectedUsers.delete(cb.value);
                });
            }
            
            updateSelectedCount();
        }

        function toggleUserSelection(userId) {
            console.log('Toggling selection for user ID:', userId, typeof userId);
            
            // Ensure userId is a string to match the API response
            const userIdStr = String(userId);
            
            if (selectedUsers.has(userIdStr)) {
                selectedUsers.delete(userIdStr);
                console.log('Removed user from selection:', userIdStr);
            } else {
                selectedUsers.add(userIdStr);
                console.log('Added user to selection:', userIdStr);
            }
            
            console.log('Current selected users:', Array.from(selectedUsers));
            
            // Update select all checkbox
            const userCheckboxes = document.querySelectorAll('.user-checkbox');
            const selectAllCheckbox = document.getElementById('selectAllCheckbox');
            const checkedCount = Array.from(userCheckboxes).filter(cb => cb.checked).length;
            
            if (checkedCount === 0) {
                selectAllCheckbox.indeterminate = false;
                selectAllCheckbox.checked = false;
            } else if (checkedCount === userCheckboxes.length) {
                selectAllCheckbox.indeterminate = false;
                selectAllCheckbox.checked = true;
            } else {
                selectAllCheckbox.indeterminate = true;
            }
            
            updateSelectedCount();
        }

        function updateSelectedCount() {
            const count = selectedUsers.size;
            const selectedCountElement = document.getElementById('selectedCount');
            const selectedCountBtn = document.getElementById('selectedCountBtn');
            const floatingDeleteBtn = document.getElementById('floatingDeleteBtn');
            
            if (selectedCountElement) {
                selectedCountElement.textContent = `${count} user${count !== 1 ? 's' : ''} selected`;
            }
            
            if (selectedCountBtn) {
                selectedCountBtn.textContent = count;
            }
            
            if (count > 0 && isBulkDeleteMode) {
                floatingDeleteBtn.classList.add('show');
            } else {
                floatingDeleteBtn.classList.remove('show');
            }
        }

        async function deleteSelectedUsers() {
            if (selectedUsers.size === 0) {
                showNotification('No users selected for deletion!', 'error');
                return;
            }

            const selectedCount = selectedUsers.size;
            if (confirm(`Are you sure you want to delete ${selectedCount} selected user${selectedCount !== 1 ? 's' : ''}? This action cannot be undone.`)) {
                try {
                    await deleteUsers(Array.from(selectedUsers));
                    
                    // Clear selection and exit bulk delete mode
                    selectedUsers.clear();
                    toggleBulkDelete();
                    
                    showNotification(`${selectedCount} user${selectedCount !== 1 ? 's' : ''} deleted successfully!`, 'success');
                } catch (error) {
                    showNotification('Failed to delete selected users', 'error');
                }
            }
        }

        // Fixed Load Users to handle API response structure
        async function loadUsers() {
            console.log('Loading users...');
            showLoading(true);
            try {
                const response = await apiRequest(`${API_BASE_URL}/all`);
                console.log('Raw loadUsers response:', response);
                
                // Handle the API response structure properly
                if (response.data) {
                    if (Array.isArray(response.data)) {
                        users = response.data;
                        console.log('Found users array:', users.length, 'users');
                    } else if (response.data.id) {
                        // Single user returned
                        users = [response.data];
                        console.log('Found single user, converted to array:', users.length, 'users');
                    } else {
                        users = [];
                        console.log('No users found in response.data');
                    }
                } else {
                    users = Array.isArray(response) ? response : [];
                    console.log('No data property found, using response directly:', users.length, 'users');
                }
                
                console.log('Final users array:', users);
                filteredUsers = [...users];
                renderUsers();
                
                if (response.message) {
                    console.log('Load users message:', response.message);
                }
            } catch (error) {
                console.error('Failed to load users:', error);
                showNotification('Failed to load users', 'error');
                users = [];
                filteredUsers = [];
                renderUsers();
            }
            showLoading(false);
        }

        // Fixed Search Users Function
        async function searchUsers(query) {
            console.log('Searching users with query:', query);
            
            // Clear timeout to prevent multiple rapid searches
            clearTimeout(searchTimeout);
            
            if (!query || !query.trim()) {
                console.log('Empty query, showing all users');
                filteredUsers = [...users];
                currentPage = 1;
                renderUsers();
                return;
            }

            showLoading(true);
            try {
                const trimmedQuery = query.trim();
                console.log('Trimmed query:', trimmedQuery);
                
                const response = await apiRequest(`${API_BASE_URL}/search?query=${encodeURIComponent(trimmedQuery)}`);
                console.log('Raw search response:', response);
                
                // Handle search response structure
                if (response.data) {
                    if (Array.isArray(response.data)) {
                        filteredUsers = response.data;
                    } else if (response.data.id) {
                        filteredUsers = [response.data];
                    } else {
                        filteredUsers = [];
                    }
                    console.log('Extracted search users from response.data:', filteredUsers.length, 'users');
                } else if (Array.isArray(response)) {
                    filteredUsers = response;
                    console.log('Using response array directly:', filteredUsers.length, 'users');
                } else {
                    filteredUsers = [];
                    console.log('No users found in search response');
                }
                
                console.log('Final filtered users:', filteredUsers);
                currentPage = 1;
                renderUsers();
            } catch (error) {
                console.error('API search failed:', error);
                console.warn('Falling back to local search');
                // Fallback to local search if API search fails
                filteredUsers = users.filter(user => 
                    user.firstName?.toLowerCase().includes(query.toLowerCase()) ||
                    user.lastName?.toLowerCase().includes(query.toLowerCase()) ||
                    user.email?.toLowerCase().includes(query.toLowerCase()) ||
                    user.userName?.toLowerCase().includes(query.toLowerCase())
                );
                console.log('Local search results:', filteredUsers.length, 'users');
                currentPage = 1;
                renderUsers();
            }
            showLoading(false);
        }

        // Fixed Create User Function to match API structure
       // Fixed Create User Function to match API structure
async function createUser(userData) {
    console.log('Creating user with data:', userData);
    try {
        // Structure payload to match API expectations
        const payload = {
            FirstName: userData.firstName, // Note: API expects PascalCase
            LastName: userData.lastName,   // Note: API expects PascalCase
            UserName: userData.email,      // API uses UserName for email
            Email: userData.email,
            PhoneNumber: userData.phoneNumber,
            Password: userData.password,   // Note: API expects PascalCase
            AccountType: userData.accountType,
            Profile: {
                Gender: userData.profile.gender,
                DateOfBirth: userData.profile.dateOfBirth,
                Nationality: userData.profile.nationality,
                ProfilePic: userData.profile.profilePic
            }
        };

        console.log('Sending create payload:', payload);
        
        const response = await apiRequest(`${API_BASE_URL}/create-user`, {
            method: 'POST',
            body: JSON.stringify(payload)
        });
        
        console.log('Create user response:', response);
        
        // Extract user from response.data
        let newUser;
        if (response.data) {
            newUser = response.data;
            console.log('Extracted new user from response.data:', newUser);
        } else {
            newUser = response;
            console.log('Using response directly as new user:', newUser);
        }
        
        users.push(newUser);
        filteredUsers = [...users];
        renderUsers();
        showNotification(response.message || 'User created successfully!', 'success');
        console.log('User created successfully, total users:', users.length);
        return newUser;
    } catch (error) {
        console.error('Failed to create user:', error);
        throw error;
    }
}

        // Fixed Update User Function to match API structure
        async function updateUser(id, userData) {
            console.log('Updating user ID:', id, 'with data:', userData);
            
            // Structure update payload (excluding password)
            const payload = {
                id: id,
                firstName: userData.firstName,
                lastName: userData.lastName,
                userName: userData.email,
                email: userData.email,
                phoneNumber: userData.phoneNumber,
                accountType: userData.accountType,
                profile: {
                    gender: userData.profile.gender,
                    dateOfBirth: userData.profile.dateOfBirth,
                    nationality: userData.profile.nationality,
                    profilePic: userData.profile.profilePic,
                    stateOfOrigin: userData.profile.stateOfOrigin,
                    address: userData.profile.address
                }
            };
            
            console.log('Sending update payload:', payload);
            
            try {
                const response = await apiRequest(`${API_BASE_URL}/update-user`, {
                    method: 'PUT',
                    body: JSON.stringify(payload)
                });
                
                console.log('Update user response:', response);
                
                // Extract updated user from response
                let updatedUser;
                if (response.data) {
                    updatedUser = response.data;
                    console.log('Extracted updated user from response.data:', updatedUser);
                } else {
                    updatedUser = response;
                    console.log('Using response directly as updated user:', updatedUser);
                }
                
                const userIndex = users.findIndex(u => u.id === id);
                console.log('Found user at index:', userIndex);
                if (userIndex !== -1) {
                    users[userIndex] = updatedUser;
                    filteredUsers = [...users];
                    renderUsers();
                    console.log('User updated in local array');
                } else {
                    console.warn('User not found in local array for update');
                }
                showNotification(response.message || 'User updated successfully!', 'success');
                return updatedUser;
            } catch (error) {
                console.error('Failed to update user:', error);
                throw error;
            }
        }

        // Fixed Delete Users Function
        async function deleteUsers(userIds) {
            console.log('Deleting users with IDs:', userIds);
            try {
                const idsToDelete = Array.isArray(userIds) ? userIds : [userIds];
                console.log('Processing delete for IDs:', idsToDelete);
                
                const payload = {
                    userIds: idsToDelete
                };
                
                console.log('Sending delete payload:', payload);
                
                const response = await apiRequest(`${API_BASE_URL}/delete-users`, {
                    method: 'DELETE',
                    body: JSON.stringify(payload)
                });
                
                console.log('Delete response:', response);
                
                // Update local data
                const beforeCount = users.length;
                users = users.filter(u => !idsToDelete.includes(u.id));
                console.log(`Removed ${beforeCount - users.length} users from local array`);
                
                filteredUsers = users.filter(user => {
                    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
                    return !searchTerm || 
                           user.firstName?.toLowerCase().includes(searchTerm) ||
                           user.lastName?.toLowerCase().includes(searchTerm) ||
                           user.email?.toLowerCase().includes(searchTerm) ||
                           user.userName?.toLowerCase().includes(searchTerm);
                });
                console.log('Updated filtered users:', filteredUsers.length);
                
                // Adjust current page if necessary
                const totalPages = Math.ceil(filteredUsers.length / usersPerPage);
                if (currentPage > totalPages && totalPages > 0) {
                    currentPage = totalPages;
                    console.log('Adjusted current page to:', currentPage);
                }
                
                renderUsers();
                showNotification(response.message || 'User(s) deleted successfully!', 'success');
                console.log('Delete operation completed successfully');
                return true;
            } catch (error) {
                console.error('Failed to delete users:', error);
                throw error;
            }
        }

        // UI Functions
        function showLoading(show) {
            const loadingIndicator = document.getElementById('loadingIndicator');
            const usersTable = document.getElementById('usersTable');
            
            if (show) {
                loadingIndicator.style.display = 'flex';
                usersTable.style.display = 'none';
            } else {
                loadingIndicator.style.display = 'none';
                usersTable.style.display = 'table';
            }
        }

        // Fixed Render Users Function to properly display API data
        function renderUsers() {
            const tbody = document.getElementById('usersTableBody');
            const tableHeader = document.getElementById('tableHeader');
            const startIndex = (currentPage - 1) * usersPerPage;
            const endIndex = startIndex + usersPerPage;
            const paginatedUsers = filteredUsers.slice(startIndex, endIndex);

            // Update table header based on bulk delete mode
            if (isBulkDeleteMode) {
                tableHeader.innerHTML = `
                    <th class="checkbox-column">
                        <input type="checkbox" class="select-all-checkbox" onchange="toggleSelectAll(this)" id="selectAllCheckbox">
                    </th>
                    <th>User</th>
                    <th>Email</th>
                    <th>Phone</th>
                    <th>Role</th>
                `;
                document.getElementById('tableContainer').classList.add('bulk-delete-mode');
                document.getElementById('bulkModeHeader').classList.add('show');
            } else {
                tableHeader.innerHTML = `
                    <th>User</th>
                    <th>Email</th>
                    <th>Phone</th>
                    <th>Role</th>
                    <th>Actions</th>
                `;
                document.getElementById('tableContainer').classList.remove('bulk-delete-mode');
                document.getElementById('bulkModeHeader').classList.remove('show');
            }

            if (paginatedUsers.length === 0) {
                const colSpan = isBulkDeleteMode ? 5 : 5;
                tbody.innerHTML = `
                    <tr>
                        <td colspan="${colSpan}" class="empty-state">
                            <h3>No users found</h3>
                            <p>Start by creating your first user</p>
                        </td>
                    </tr>
                `;
                return;
            }

            tbody.innerHTML = paginatedUsers.map(user => {
                const defaultAvatar = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNTAiIGhlaWdodD0iNTAiIHZpZXdCb3g9IjAgMCA1MCA1MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPGNpcmNsZSBjeD0iMjUiIGN5PSIyNSIgcj0iMjUiIGZpbGw9IiNFMkU4RjAiLz4KPHN2ZyB3aWR0aD0iMjQiIGhlaWdodD0iMjQiIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiBzdHlsZT0idHJhbnNmb3JtOiB0cmFuc2xhdGUoMTNweCwgMTNweCkiPgo8cGF0aCBkPSJNMjAgMjFWMTlBNCA0IDAgMCAwIDE2IDE1SDhBNCA0IDAgMCAwIDQgMTlWMjEiIHN0cm9rZT0iIzY0NzQ4QiIgc3Ryb2tlLXdpZHRoPSIyIiBzdHJva2UtbGluZWNhcD0icm91bmQiIHN0cm9rZS1saW5lam9pbj0icm91bmQiLz4KPGF0aCBkPSJNMTIgMTFBNCA0IDAgMSAwIDEyIDdBNCA0IDAgMCAwIDEyIDExWiIgc3Ryb2tlPSIjNjQ3NDhCIiBzdHJva2Utd2lkdGg9IjIiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIgc3Ryb2tlLWxpbmVqb2luPSJyb3VuZCIvPgo8L3N2Zz4KPC9zdmc+';
                
                // Extract data based on actual API structure
                const firstName = user.firstName || '';
                const lastName = user.lastName || '';
                const email = user.email || user.userName || '';
                const phone = user.phoneNumber || '';
                const nationality = user.profile?.nationality || 'N/A';
                const role = user.accountType || 'User';
                const picture = user.profile?.profilePic || defaultAvatar;
                
                console.log('Rendering user:', { firstName, lastName, email, phone, role });
                
                if (isBulkDeleteMode) {
                    return `
                        <tr>
                            <td class="checkbox-column">
                                <input type="checkbox" class="user-checkbox" value="${user.id}" onchange="toggleUserSelection('${user.id}')" ${selectedUsers.has(user.id) ? 'checked' : ''}>
                            </td>
                            <td>
                                <div class="user-info">
                                    <img src="${picture}" alt="${firstName}" class="user-avatar" onerror="this.src='${defaultAvatar}'">
                                    <div class="user-details">
                                        <h4>${firstName} ${lastName}</h4>
                                        <p>${nationality}</p>
                                    </div>
                                </div>
                            </td>
                            <td>${email}</td>
                            <td>${phone}</td>
                            <td>
                                <span class="role-badge role-${role.toLowerCase()}">${role}</span>
                            </td>
                        </tr>
                    `;
                } else {
                    return `
                        <tr>
                            <td>
                                <div class="user-info">
                                    <img src="${picture}" alt="${firstName}" class="user-avatar" onerror="this.src='${defaultAvatar}'">
                                    <div class="user-details">
                                        <h4>${firstName} ${lastName}</h4>
                                        <p>${nationality}</p>
                                    </div>
                                </div>
                            </td>
                            <td>${email}</td>
                            <td>${phone}</td>
                            <td>
                                <span class="role-badge role-${role.toLowerCase()}">${role}</span>
                            </td>
                            <td>
                                <div class="actions">
                                    <button class="btn btn-edit" onclick="editUser('${user.id}')">Edit</button>
                                    <button class="btn btn-danger" onclick="deleteUser('${user.id}')">Delete</button>
                                </div>
                            </td>
                        </tr>
                    `;
                }
            }).join('');

            renderPagination();
            updateSelectedCount();
        }

        function renderPagination() {
            const totalPages = Math.ceil(filteredUsers.length / usersPerPage);
            const pagination = document.getElementById('pagination');
            
            if (totalPages <= 1) {
                pagination.innerHTML = '';
                return;
            }

            let paginationHTML = `
                <button onclick="changePage(${currentPage - 1})" ${currentPage === 1 ? 'disabled' : ''}>Previous</button>
            `;

            for (let i = 1; i <= totalPages; i++) {
                paginationHTML += `
                    <button onclick="changePage(${i})" ${i === currentPage ? 'class="active"' : ''}>${i}</button>
                `;
            }

            paginationHTML += `
                <button onclick="changePage(${currentPage + 1})" ${currentPage === totalPages ? 'disabled' : ''}>Next</button>
            `;

            pagination.innerHTML = paginationHTML;
        }

        function changePage(page) {
            const totalPages = Math.ceil(filteredUsers.length / usersPerPage);
            if (page < 1 || page > totalPages) return;
            
            currentPage = page;
            renderUsers();
        }

        // Fixed Search Handler with proper debouncing
        function handleSearch() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                const searchInput = document.getElementById('searchInput');
                const searchTerm = searchInput.value;
                console.log('Search input value:', searchTerm, 'Length:', searchTerm.length);
                searchUsers(searchTerm);
            }, 300);
        }

        function openCreateModal() {
            isEditMode = false;
            editingUserId = null;
            document.getElementById('modalTitle').textContent = 'Create New User';
            document.getElementById('submitBtn').textContent = 'Create User';
            document.getElementById('userForm').reset();
            document.getElementById('imagePreview').style.display = 'none';
            document.getElementById('imageUploadText').style.display = 'block';
            document.getElementById('stateGroup').style.display = 'none';
            document.getElementById('addressGroup').style.display = 'none';
            document.getElementById('passwordGroup').style.display = 'block';
            currentImageData = null;
            clearErrors();
            document.getElementById('userModal').style.display = 'block';
        }

        // Fixed Edit User Function to handle API structure
        function editUser(userId) {
            console.log('Editing user with ID:', userId);
            const user = users.find(u => u.id === userId);
            if (!user) {
                console.error('User not found for editing:', userId);
                return;
            }

            console.log('Found user for editing:', user);
            isEditMode = true;
            editingUserId = userId;
            document.getElementById('modalTitle').textContent = 'Edit User';
            document.getElementById('submitBtn').textContent = 'Update User';
            
            // Hide password field for editing
            document.getElementById('passwordGroup').style.display = 'none';
            
            // Show additional fields for editing
            document.getElementById('stateGroup').style.display = 'block';
            document.getElementById('addressGroup').style.display = 'block';

            // Populate form with API structure data
            document.getElementById('firstName').value = user.firstName || '';
            document.getElementById('lastName').value = user.lastName || '';
            document.getElementById('email').value = user.email || user.userName || '';
            document.getElementById('phone').value = user.phoneNumber || '';
            document.getElementById('gender').value = user.profile?.gender || '';
            document.getElementById('dateOfBirth').value = user.profile?.dateOfBirth || '';
            document.getElementById('nationality').value = user.profile?.nationality || '';
            document.getElementById('role').value = user.accountType || '';
            document.getElementById('stateOfOrigin').value = user.profile?.stateOfOrigin || '';
            document.getElementById('address').value = user.profile?.address || '';

            console.log('Form populated with values:', {
                firstName: document.getElementById('firstName').value,
                lastName: document.getElementById('lastName').value,
                email: document.getElementById('email').value,
                phone: document.getElementById('phone').value,
                gender: document.getElementById('gender').value,
                role: document.getElementById('role').value
            });

            // Handle image preview
            const profilePic = user.profile?.profilePic;
            if (profilePic) {
                document.getElementById('imagePreview').src = profilePic;
                document.getElementById('imagePreview').style.display = 'block';
                document.getElementById('imageUploadText').style.display = 'none';
                currentImageData = profilePic;
                console.log('Set profile picture for editing');
            } else {
                console.log('No profile picture found');
            }

            clearErrors();
            document.getElementById('userModal').style.display = 'block';
        }

        function closeModal() {
            document.getElementById('userModal').style.display = 'none';
            document.getElementById('userForm').reset();
            document.getElementById('imagePreview').style.display = 'none';
            document.getElementById('imageUploadText').style.display = 'block';
            currentImageData = null;
            clearErrors();
        }

        function deleteUser(userId) {
            if (confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
                deleteUsers([userId]);
            }
        }

        function clearErrors() {
            const errorElements = document.querySelectorAll('.error-message');
            errorElements.forEach(element => {
                element.textContent = '';
            });
        }

        function showFieldError(fieldId, message) {
            const errorElement = document.getElementById(`${fieldId}Error`);
            if (errorElement) {
                errorElement.textContent = message;
            }
        }

        function validateForm() {
            clearErrors();
            let isValid = true;

            const requiredFields = {
                firstName: 'First Name',
                lastName: 'Last Name',
                email: 'Email',
                phone: 'Phone',
                gender: 'Gender',
                dateOfBirth: 'Date of Birth',
                nationality: 'Nationality',
                role: 'Role'
            };

            // Add password to required fields if creating new user
            if (!isEditMode) {
                requiredFields.password = 'Password';
            }

            // Check required fields
            for (const [fieldId, fieldName] of Object.entries(requiredFields)) {
                const value = document.getElementById(fieldId).value.trim();
                if (!value) {
                    showFieldError(fieldId, `${fieldName} is required`);
                    isValid = false;
                }
            }

            // Email validation
            const email = document.getElementById('email').value.trim();
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (email && !emailRegex.test(email)) {
                showFieldError('email', 'Please enter a valid email address');
                isValid = false;
            }

            // Phone validation
            const phone = document.getElementById('phone').value.trim();
            if (phone && phone.length < 10) {
                showFieldError('phone', 'Phone number must be at least 10 digits');
                isValid = false;
            }

            // Password validation (only for new users)
            if (!isEditMode) {
                const password = document.getElementById('password').value;
                if (password && password.length < 6) {
                    showFieldError('password', 'Password must be at least 6 characters');
                    isValid = false;
                }
            }

            // Date validation
            const dob = new Date(document.getElementById('dateOfBirth').value);
            const today = new Date();
            const age = today.getFullYear() - dob.getFullYear();
            if (dob && age < 13) {
                showFieldError('dateOfBirth', 'User must be at least 13 years old');
                isValid = false;
            }

            // Check for duplicate email (excluding current user in edit mode)
            if (email) {
                const existingUser = users.find(u => 
                    (u.email && u.email.toLowerCase() === email.toLowerCase()) ||
                    (u.userName && u.userName.toLowerCase() === email.toLowerCase()) &&
                    (!isEditMode || u.id !== editingUserId)
                );
                
                if (existingUser) {
                    showFieldError('email', 'A user with this email already exists');
                    isValid = false;
                }
            }

            return isValid;
        }

        // Fixed Form Submit Handler to match API structure
        async function handleFormSubmit(e) {
            e.preventDefault();
            console.log('Form submission started');
            
            if (!validateForm()) {
                console.log('Form validation failed');
                return;
            }

            const submitBtn = document.getElementById('submitBtn');
            const originalText = submitBtn.textContent;
            submitBtn.disabled = true;
            submitBtn.textContent = isEditMode ? 'Updating...' : 'Creating...';
            
            try {
                // Build payload structure to match API expectations
                const userData = {
                    firstName: document.getElementById('firstName').value.trim(),
                    lastName: document.getElementById('lastName').value.trim(),
                    email: document.getElementById('email').value.trim(),
                    phoneNumber: document.getElementById('phone').value.trim(),
                    accountType: document.getElementById('role').value,
                    profile: {
                        gender: document.getElementById('gender').value,
                        dateOfBirth: document.getElementById('dateOfBirth').value,
                        nationality: document.getElementById('nationality').value.trim(),
                        profilePic: currentImageData
                    }
                };

                // Add password ONLY for new users
                if (!isEditMode) {
                    userData.password = document.getElementById('password').value;
                    console.log('Adding password for new user');
                } else {
                    console.log('EDIT MODE: Not including password');
                    // Add additional profile fields for edit mode
                    userData.profile.stateOfOrigin = document.getElementById('stateOfOrigin').value.trim();
                    userData.profile.address = document.getElementById('address').value.trim();
                }

                console.log('Final userData payload:', userData);

                if (isEditMode) {
                    console.log('Calling updateUser for ID:', editingUserId);
                    await updateUser(editingUserId, userData);
                } else {
                    console.log('Calling createUser');
                    await createUser(userData);
                }

                console.log('Form submission completed successfully');
                closeModal();
            } catch (error) {
                console.error('Form submission failed:', error);
                showNotification('Failed to save user. Please try again.', 'error');
            } finally {
                submitBtn.disabled = false;
                submitBtn.textContent = originalText;
            }
        }
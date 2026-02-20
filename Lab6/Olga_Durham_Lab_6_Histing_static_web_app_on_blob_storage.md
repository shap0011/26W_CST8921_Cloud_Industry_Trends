# CST8921 – Cloud Industry Trends

## Lab 6 – Hosting static web app on blob storage 

**Completed by: Olga Durham**
**St#: 040687883**

---

### Introduction 

In this lab, students will explore and understand cloud storage account. Cloud Storage Account is a storage offering that is designed to support and enhance the capabilities of systems that are serving documents, files, images, video and logs files at scale with features such as high availability and replications. The primary service of the storage account, Blob storage supports low-cost and tiered storage with high availability and strong consistency to provide fast and reliable disaster recovery solutions for a massive amount of data.
Static website is one of the newly introduced capabilities of Cloud Storage Account that supports hosting static HTML-based website and associated assets to host website. The hosting does not require any rendering or management of the hosting platform. If companies are looking to host a simple landing page or static content, a Static website with Cloud Storage might be the best option for the organization.

---

### Objective

The goal of this lab activity is to familiarize students with the concepts, techniques and containerized cloud solutions using AWS/Azure/GCP

---

### Prerequisites:

- Basic understanding of cloud storage types
- A computer with internet access 
- Windows or mac machine
- Web browser
- Cloud portal access with any cloud service providers (AWS, Azure, GCP)

---

### Lab Activity Overview:

#### Part A: Environment Setup

##### Step 1: Launch Visual Studio Code

- Open **Visual Studio Code** on your computer.

##### Step 2: Install Azure Storage Extension

1. Open the **Extensions** panel.
2. Search for **Azure Storage.**
3. Install the extension published by Microsoft.

#### Part B: Azure Portal Configuration

##### Step 3: Log in to Azure Portal

- Navigate to: https://portal.azure.com
- Sign in using your Azure account.

##### Step 4: Create a Storage Account

1. In the Azure Portal, search for **Storage accounts.**
2. Click **Create.**
3. Select:
    - Subscription
    - Resource Group (create a new one if required)
4. Enter:
    - Storage account name (must be globally unique)
    - Region (closest to your location)
5. Leave all other settings as default.
6. Click **Review + Create,** then **Create.**

##### Step 5: Enable Static Website Hosting

1. Open the newly created storage account.
2. In the left navigation pane, select **Static website.**
3. Set **Static website** to **Enabled.**
4. Configure:
    - **Index document name:** index.html
    - **Error document path:** 404.html
5. Click **Save.**

Once saved, Azure will generate a **Primary endpoint URL** for your website.

#### Part C: Create the Static Website Locally

##### Step 6: Create Website Folder

1. Create a new folder on your local machine named:
2. `mywebsite`
3. Open this folder in **Visual Studio Code.**

##### Step 7: Create index.html

1. Inside the `mywebsite` folder, create a file named:
2. index.html
3. Paste the following content and save:
<!DOCTYPE html>
<html>
  <body>
    <h1>Hello World!</h1>
  </body>
</html>

##### Step 8: Create 404.html

1. Create another file named:
2. 404.html
3. Paste the following content and save:

<!DOCTYPE html>
<html>
  <body>
    <h1>404</h1>
  </body>
</html>

#### Part D: Deploy Website to Azure

##### Step 9: Deploy to Static Website

1. In VS Code Explorer, **right-click** the `mywebsite` folder.
2. Select **Deploy to Static Website.**
3. Choose:
    - Azure subscription
    - Storage account you created earlier
4. Wait for the deployment to complete.

##### Step 10: Validate Deployment

1. Return to the **Azure Portal.**
2. Open **Storage Account → Static website.**
3. Click the **Primary endpoint URL.**
4. Confirm:
    - “Hello World!” page loads successfully
    - Navigating to a non-existent page displays the **404** page

#### Part E: Cleanup (Mandatory)

##### Step 11: Delete Resources

1. In Azure Portal, navigate to **Resource Groups.**
2. Select the resource group used for this lab.
3. Click **Delete resource group.**
4. Confirm deletion.

This step ensures no unnecessary cloud resources remain active.

##### 7. Expected Outcome

By the end of this lab, students should be able to:

- Successfully host a static website using Azure Blob Storage
- Access the site through a public web endpoint
- Understand the benefits of serverless static hosting
- Deploy cloud resources using Visual Studio Code

##### 8. Lab Report Submission Requirements

For grading, students must submit a **Lab Report** via the **Assignments tab in Brightspace,** including:

##### Lab Report Must Contain

- Objective of the lab (in your own words)
- Screenshots of:
- Storage account creation
- Static website configuration
- Successful website output
- Steps followed during deployment
- Issues encountered (if any) and how they were resolved
- Final observations and learning outcomes

---

### Important Notes

For grading prepare a lab report with your findings and analysis and share that in an Assignments tab in Brightspace.
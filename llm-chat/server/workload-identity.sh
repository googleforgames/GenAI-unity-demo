#!/usr/bin/env bash
# Setup workload identity stuff

export GCS_BUCKET_PROJECT_ID=gss-arena
export GCP_SA_NAME=gameprompt-worker
export STORAGE_ROLE=roles/storage.objectViewer
export BUCKET_NAME=gss-arena-llm-storage
export K8S_NAMESPACE=gameprompt
export K8S_SA_NAME=gameprompt-worker
export CLUSTER_PROJECT_ID=gss-arena

gcloud iam service-accounts create ${GCP_SA_NAME} --project=${GCS_BUCKET_PROJECT_ID}

gcloud storage buckets add-iam-policy-binding gs://${BUCKET_NAME} \
    --member "serviceAccount:${GCP_SA_NAME}@${GCS_BUCKET_PROJECT_ID}.iam.gserviceaccount.com" \
    --role "${STORAGE_ROLE}"

kubectl create namespace ${K8S_NAMESPACE}
kubectl create serviceaccount ${K8S_SA_NAME} --namespace ${K8S_NAMESPACE}

gcloud iam service-accounts add-iam-policy-binding ${GCP_SA_NAME}@${GCS_BUCKET_PROJECT_ID}.iam.gserviceaccount.com \
    --role roles/iam.workloadIdentityUser \
    --member "serviceAccount:${CLUSTER_PROJECT_ID}.svc.id.goog[${K8S_NAMESPACE}/${K8S_SA_NAME}]"

kubectl annotate serviceaccount ${K8S_SA_NAME} \
    --namespace ${K8S_NAMESPACE} \
    iam.gke.io/gcp-service-account=${GCP_SA_NAME}@${GCS_BUCKET_PROJECT_ID}.iam.gserviceaccount.com
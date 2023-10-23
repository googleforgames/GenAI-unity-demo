package coordination

import (
	"context"
	"log"

	v1 "k8s.io/api/autoscaling/v1"
	metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"
	"k8s.io/client-go/kubernetes"
)

type GamePromptK8SContextKey string

var K8SCtxKey GamePromptK8SContextKey = "gpc-k8s"

const gameprompt_namespace = "gameprompt"

func GetReplicaCount(ctx context.Context, deployment string) *v1.Scale {
	clientset := ctx.Value(K8SCtxKey).(*kubernetes.Clientset)

	deps, err := clientset.AppsV1().Deployments(gameprompt_namespace).GetScale(ctx, deployment, metav1.GetOptions{})
	if err != nil {
		log.Fatalln("[Coordination] Cannot get replica count from Kubernetes: ", err)
	}
	return deps
}

func GetServiceIP(ctx context.Context, service string) string {
	clientset := ctx.Value(K8SCtxKey).(*kubernetes.Clientset)

	svc, err := clientset.CoreV1().Services(gameprompt_namespace).Get(ctx, service, metav1.GetOptions{})
	if err != nil {
		log.Fatalln("[Coordination] Cannot get service IP from Kubernetes: ", err)
	}
	return svc.Status.LoadBalancer.Ingress[0].IP
}

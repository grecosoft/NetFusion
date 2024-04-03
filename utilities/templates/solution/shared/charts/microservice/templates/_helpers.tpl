{{/* Naming Conventions */}}

{{- define "solution.name"  }}
{{- .Values.solution | lower }}
{{- end }}

{{- define "microservice.name"  }}
{{- .Values.name | lower }}
{{- end }}

{{- define "microservice.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "microservice.image" }}
{{- printf "%s/%s/%s:%s" .Values.image.registry (include "solution.name" .) (include "microservice.name" .) .Values.image.tag }}
{{- end }}

{{/* Common labels */}}

{{- define "microservice.labels" -}}
helm.sh/chart: {{ include "microservice.chart" . }}
{{ include "microservice.selectorLabels" . }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
{{- if .Values.extraLabels }}
{{ toYaml .Values.extraLabels }}
{{- end }}
{{- end }}

{{/* Selector labels */}}

{{- define "microservice.selectorLabels" -}}
app.kubernetes.io/name: {{ include "microservice.name" . }}
{{- end }}
















import { CrudPanel } from '../../components'

export function StudentsSection({ data, openCreate, openEdit, deleteEntity, getPanelProps }) {
  return (
    <>
      <CrudPanel entityKey="students" rows={data.students} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('students-students')} />
      <CrudPanel entityKey="contracts" rows={data.contracts} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('students-contracts')} />
    </>
  )
}
